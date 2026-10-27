using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class AchievementsApiTests(ContentApiFixture api)
{
    private async Task<(HttpClient Client, AuthResponse Auth)> NewUserClientAsync()
    {
        var client = api.CreateBrowserClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"user_{suffix}", $"user_{suffix}@example.com", "Senha123", "Senha123"));
        response.EnsureSuccessStatusCode();
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return (client, auth);
    }

    private static async Task<List<UserAchievementDto>> MyAchievementsAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<List<UserAchievementDto>>("/api/users/me/achievements", ContentApiFixture.JsonOptions))!;

    [SkippableFact]
    public async Task Catalog_IsPublicAndOrderedFromRegistration()
    {
        api.Database.SkipIfUnavailable();

        var catalog = await api.GetJsonAsync<List<AchievementDto>>("/api/achievements");

        Assert.Equal(6, catalog.Count);
        Assert.Equal("Primeiro passo", catalog[0].Name);
        Assert.Equal(["Curioso", "Explorador"], catalog.Skip(1).Take(2).Select(a => a.Name));
        Assert.All(catalog, a => Assert.True(a.ExperienceReward > 0));
    }

    [SkippableFact]
    public async Task Register_UnlocksFirstStepAndReturnsTheReward()
    {
        api.Database.SkipIfUnavailable();

        var (client, auth) = await NewUserClientAsync();

        var reward = auth.Reward;
        Assert.NotNull(reward);
        Assert.Equal("Primeiro passo", Assert.Single(reward.AchievementsUnlocked).Name);
        Assert.Equal((10, 10), (reward.XpGained, auth.User.ExperiencePoints));

        var achievements = await MyAchievementsAsync(client);
        var firstStep = achievements.Single(a => a.Name == "Primeiro passo");
        Assert.True(firstStep.IsUnlocked);
        Assert.NotNull(firstStep.UnlockedAt);
        Assert.Single(achievements, a => a.IsUnlocked);
    }

    [SkippableFact]
    public async Task Login_DoesNotRepeatTheReward()
    {
        api.Database.SkipIfUnavailable();
        var (_, auth) = await NewUserClientAsync();

        var response = await api.CreateBrowserClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(auth.User.Email, "Senha123"));
        var login = (await response.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;

        Assert.Null(login.Reward);
        Assert.Equal(10, login.User.ExperiencePoints);
    }

    [SkippableFact]
    public async Task DiscoveringAllNations_UnlocksHistorianOnTheLastOne()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var nations = await api.GetJsonAsync<List<NationDto>>("/api/nations");

        var rewards = new List<RewardResult?>();
        foreach (var nation in nations)
        {
            var response = await client.PostAsync($"/api/nations/{nation.Id}/discovery", null);
            response.EnsureSuccessStatusCode();
            rewards.Add((await response.Content.ReadFromJsonAsync<DiscoveryResultDto>(ContentApiFixture.JsonOptions))!.Reward);
        }

        Assert.All(rewards.SkipLast(1), r => Assert.Empty(r!.AchievementsUnlocked));
        var last = rewards[^1]!;
        Assert.Equal("Historiador", Assert.Single(last.AchievementsUnlocked).Name);
        Assert.Equal(10 + 50, last.XpGained);

        var historian = (await MyAchievementsAsync(client)).Single(a => a.Name == "Historiador");
        Assert.True(historian.IsUnlocked);
        Assert.Equal((nations.Count, nations.Count), (historian.Progress, historian.Target));
    }

    [SkippableFact]
    public async Task LockedAchievements_ShowProgress()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var page = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?pageSize=3");
        foreach (var character in page.Items)
            await client.PostAsync($"/api/characters/{character.Id}/discovery", null);

        var achievements = await MyAchievementsAsync(client);

        var curious = achievements.Single(a => a.Name == "Curioso");
        Assert.False(curious.IsUnlocked);
        Assert.Null(curious.UnlockedAt);
        Assert.Equal((3, 10), (curious.Progress, curious.Target));

        // Quizzes ainda não existem (Fase 9): a conquista aparece bloqueada, sem progresso.
        var scholar = achievements.Single(a => a.Name == "Conhecedor de Avatar");
        Assert.Equal((0, 100), (scholar.Progress, scholar.Target));
    }
}
