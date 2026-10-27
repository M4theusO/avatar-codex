using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class ProgressApiTests(ContentApiFixture api)
{
    /// <summary>Cliente com um usuário novo já autenticado (header Bearer).</summary>
    private async Task<(HttpClient Client, AuthResponse Auth)> NewUserClientAsync()
    {
        var client = api.CreateBrowserClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"user_{suffix}", $"user_{suffix}@example.com", "Senha123", "Senha123"));
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return (client, auth);
    }

    private async Task<int> CharacterIdAsync(string name)
    {
        var page = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>($"/api/characters?search={name}");
        return page.Items.First(c => c.Name == name).Id;
    }

    private async Task<int> NationIdAsync(string name)
    {
        var nations = await api.GetJsonAsync<List<NationDto>>("/api/nations");
        return nations.Single(n => n.Name == name).Id;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task CharacterDiscovery_FirstTimeGrantsXpThenIsIdempotent()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var zuko = await CharacterIdAsync("Zuko");

        var first = await ReadAsync<DiscoveryResultDto>(await client.PostAsync($"/api/characters/{zuko}/discovery", null));
        var second = await ReadAsync<DiscoveryResultDto>(await client.PostAsync($"/api/characters/{zuko}/discovery", null));

        // O cadastro já rendeu 10 XP ("Primeiro passo").
        Assert.Equal(10, auth.User.ExperiencePoints);
        Assert.True(first.IsNew);
        Assert.Equal(10, first.Reward!.XpGained);
        Assert.Equal(20, first.Reward.TotalXp);
        Assert.Equal(new DiscoveryResultDto(false, null), second);

        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);
        Assert.Equal(20, me!.ExperiencePoints);
    }

    [SkippableFact]
    public async Task CharacterDiscovery_ConcurrentRequests_GrantXpExactlyOnce()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var aang = await CharacterIdAsync("Aang");

        // Dez requisições simultâneas para a mesma descoberta (ex.: vários cliques/abas).
        var responses = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => client.PostAsync($"/api/characters/{aang}/discovery", null)));
        var results = await Task.WhenAll(responses.Select(ReadAsync<DiscoveryResultDto>));

        Assert.Single(results, r => r.IsNew);
        Assert.All(results.Where(r => !r.IsNew), r => Assert.Null(r.Reward));

        await using var db = api.Database.CreateContext();
        var userId = auth.User.Id;
        Assert.Equal(1, await db.CharacterDiscoveries.CountAsync(d => d.UserId == userId));
        Assert.Equal(1, await db.ExperienceTransactions.CountAsync(t => t.UserId == userId && t.Type == ExperienceType.CharacterDiscovered));
        Assert.Equal(20, await db.Users.Where(u => u.Id == userId).Select(u => u.ExperiencePoints).SingleAsync());
    }

    [SkippableFact]
    public async Task DifferentDiscoveriesAtTheSameTime_DoNotLoseXpAndUnlockCuriousOnce()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var page = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?pageSize=10");

        await Task.WhenAll(page.Items.Select(c => client.PostAsync($"/api/characters/{c.Id}/discovery", null)));

        // Todas as requisições avaliam o "Curioso" (10 personagens); só uma pode desbloqueá-lo (RN-05).
        // 10 (Primeiro passo) + 10 × 10 (descobertas) + 50 (Curioso).
        await using var db = api.Database.CreateContext();
        var userId = auth.User.Id;
        Assert.Equal(160, await db.Users.Where(u => u.Id == userId).Select(u => u.ExperiencePoints).SingleAsync());
        Assert.Equal(1, await db.UserAchievements.CountAsync(u => u.UserId == userId && u.Achievement.Code == "CURIOUS"));
        Assert.Equal(160, await db.ExperienceTransactions.Where(t => t.UserId == userId).SumAsync(t => t.Amount));
    }

    [SkippableFact]
    public async Task NationDiscovery_GrantsTenXpOnce()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var fireNation = await NationIdAsync("Nação do Fogo");

        var first = await ReadAsync<DiscoveryResultDto>(await client.PostAsync($"/api/nations/{fireNation}/discovery", null));
        var second = await ReadAsync<DiscoveryResultDto>(await client.PostAsync($"/api/nations/{fireNation}/discovery", null));

        Assert.True(first.IsNew);
        Assert.Equal(10, first.Reward!.XpGained);
        Assert.False(second.IsNew);

        await using var db = api.Database.CreateContext();
        var transaction = await db.ExperienceTransactions.SingleAsync(t =>
            t.UserId == auth.User.Id && t.Type != ExperienceType.AchievementUnlocked);
        Assert.Equal(ExperienceType.NationDiscovered, transaction.Type);
        Assert.Equal(fireNation, transaction.SourceId);
    }

    [SkippableTheory]
    [InlineData("/api/characters/999999/discovery")]
    [InlineData("/api/nations/999999/discovery")]
    public async Task Discovery_UnknownResource_ReturnsNotFound(string url)
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync(url, null)).StatusCode);
    }

    [SkippableTheory]
    [InlineData("POST", "/api/characters/1/discovery")]
    [InlineData("POST", "/api/nations/1/discovery")]
    [InlineData("POST", "/api/characters/1/favorite")]
    [InlineData("DELETE", "/api/characters/1/favorite")]
    [InlineData("GET", "/api/users/me/favorites")]
    [InlineData("GET", "/api/users/me/discoveries")]
    [InlineData("GET", "/api/users/me/statistics")]
    [InlineData("GET", "/api/users/me/experience")]
    [InlineData("GET", "/api/users/me/achievements")]
    [InlineData("GET", "/api/users/me/quiz-history")]
    public async Task UserEndpoints_WithoutToken_ReturnUnauthorized(string method, string url)
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateBrowserClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task Favorites_AddListAndRemove()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var toph = await CharacterIdAsync("Toph Beifong");
        var iroh = await CharacterIdAsync("Iroh");

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/characters/{toph}/favorite", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/characters/{toph}/favorite", null)).StatusCode);
        await client.PostAsync($"/api/characters/{iroh}/favorite", null);

        var favorites = await client.GetFromJsonAsync<PagedResult<CharacterListItemDto>>("/api/users/me/favorites", ContentApiFixture.JsonOptions);
        Assert.Equal(2, favorites!.TotalItems);
        Assert.Equal(["Iroh", "Toph Beifong"], favorites.Items.Select(c => c.Name));
        Assert.All(favorites.Items, c => Assert.True(c.IsFavorite));

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/characters/{toph}/favorite")).StatusCode);
        favorites = await client.GetFromJsonAsync<PagedResult<CharacterListItemDto>>("/api/users/me/favorites", ContentApiFixture.JsonOptions);
        Assert.Equal(["Iroh"], favorites!.Items.Select(c => c.Name));
    }

    [SkippableFact]
    public async Task Favorite_UnknownCharacter_ReturnsNotFound()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync("/api/characters/999999/favorite", null)).StatusCode);
    }

    [SkippableFact]
    public async Task ContentEndpoints_WithToken_MarkFavoritesAndDiscoveries()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var zuko = await CharacterIdAsync("Zuko");
        await client.PostAsync($"/api/characters/{zuko}/favorite", null);
        await client.PostAsync($"/api/characters/{zuko}/discovery", null);

        var detail = await client.GetFromJsonAsync<CharacterDetailsDto>($"/api/characters/{zuko}", ContentApiFixture.JsonOptions);
        Assert.True(detail!.IsFavorite);
        Assert.True(detail.IsDiscovered);

        var list = await client.GetFromJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?search=Zuko", ContentApiFixture.JsonOptions);
        Assert.True(list!.Items.Single(c => c.Id == zuko).IsFavorite);

        var fireNation = await NationIdAsync("Nação do Fogo");
        var nation = await client.GetFromJsonAsync<NationDetailsDto>($"/api/nations/{fireNation}", ContentApiFixture.JsonOptions);
        Assert.True(nation!.Characters.Single(c => c.Id == zuko).IsDiscovered);

        // Visitante: sem marcas.
        var anonymous = await api.GetJsonAsync<CharacterDetailsDto>($"/api/characters/{zuko}");
        Assert.False(anonymous.IsFavorite);
        Assert.False(anonymous.IsDiscovered);
    }

    [SkippableFact]
    public async Task ExperienceHistoryAndLevelProgress_ReflectRewards()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        Assert.Equal((1, 0, 100, 10), (auth.User.Level, auth.User.CurrentLevelXp, auth.User.NextLevelXp, auth.User.LevelProgress));

        var zuko = await CharacterIdAsync("Zuko");
        var fireNation = await NationIdAsync("Nação do Fogo");
        var discovery = await ReadAsync<DiscoveryResultDto>(await client.PostAsync($"/api/characters/{zuko}/discovery", null));
        await client.PostAsync($"/api/nations/{fireNation}/discovery", null);

        Assert.Equal((20, 0, 100, 20), (discovery.Reward!.TotalXp, discovery.Reward.CurrentLevelXp, discovery.Reward.NextLevelXp, discovery.Reward.LevelProgress));

        var history = await client.GetFromJsonAsync<PagedResult<ExperienceTransactionDto>>(
            "/api/users/me/experience", ContentApiFixture.JsonOptions);
        Assert.Equal(3, history!.TotalItems);
        Assert.Equal(
            [
                (ExperienceType.NationDiscovered, "Descobriu Nação do Fogo"),
                (ExperienceType.CharacterDiscovered, "Descobriu Zuko"),
                (ExperienceType.AchievementUnlocked, "Conquista: Primeiro passo")
            ],
            history.Items.Select(t => (t.Type, t.Description)));

        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);
        Assert.Equal((30, 30), (me!.ExperiencePoints, me.LevelProgress));
    }

    [SkippableFact]
    public async Task DiscoveriesAndStatistics_ReflectUserActivity()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var zuko = await CharacterIdAsync("Zuko");
        var fireNation = await NationIdAsync("Nação do Fogo");
        await client.PostAsync($"/api/characters/{zuko}/discovery", null);
        await client.PostAsync($"/api/nations/{fireNation}/discovery", null);
        await client.PostAsync($"/api/characters/{zuko}/favorite", null);

        var history = await client.GetFromJsonAsync<PagedResult<DiscoveryDto>>("/api/users/me/discoveries", ContentApiFixture.JsonOptions);
        Assert.Equal(2, history!.TotalItems);
        Assert.Equal((DiscoveryKind.Nation, "Nação do Fogo"), (history.Items[0].Kind, history.Items[0].Name));
        Assert.Equal((DiscoveryKind.Character, "Zuko"), (history.Items[1].Kind, history.Items[1].Name));

        var stats = await client.GetFromJsonAsync<UserStatisticsDto>("/api/users/me/statistics", ContentApiFixture.JsonOptions);
        Assert.Equal(1, stats!.DiscoveredCharacters);
        Assert.Equal(1, stats.DiscoveredNations);
        Assert.Equal(1, stats.Favorites);
        Assert.Equal(5, stats.TotalNations);
        Assert.True(stats.TotalCharacters >= 37);
        Assert.Equal(1, stats.UnlockedAchievements);
        Assert.Equal(6, stats.TotalAchievements);
    }
}
