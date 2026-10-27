using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class DashboardApiTests(ContentApiFixture api)
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

    private async Task<int> CharacterIdAsync(string name)
    {
        var page = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>($"/api/characters?search={name}");
        return page.Items.First(c => c.Name == name).Id;
    }

    private static async Task<DashboardDto> GetDashboardAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/users/me/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<DashboardDto>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task Dashboard_RequiresLogin()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateClient().GetAsync("/api/users/me/dashboard");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task Dashboard_NewUser_ShowsFirstStepsAndRecommendations()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var quizzes = await api.GetJsonAsync<List<QuizListItemDto>>("/api/quizzes");

        var dashboard = await GetDashboardAsync(client);

        Assert.Equal(auth.User.Id, dashboard.User.Id);
        Assert.Equal((10, 1), (dashboard.User.ExperiencePoints, dashboard.User.Level));
        Assert.Equal((0, 37), (dashboard.Statistics.DiscoveredCharacters, dashboard.Statistics.TotalCharacters));
        // Só o "Primeiro passo", ganho no cadastro.
        var firstStep = Assert.Single(dashboard.RecentAchievements);
        Assert.True(firstStep.IsUnlocked);
        Assert.NotNull(dashboard.NextAchievement);
        Assert.False(dashboard.NextAchievement.IsUnlocked);
        Assert.Empty(dashboard.RecentDiscoveries);
        Assert.Empty(dashboard.Favorites);
        Assert.Equal(4, dashboard.Recommendations.Count);
        Assert.All(dashboard.Recommendations, c => Assert.False(c.IsDiscovered));
        Assert.Equal(quizzes[0].Id, dashboard.FeaturedQuiz!.Id);
        Assert.Null(dashboard.FeaturedQuiz.BestResult);

        // A rotação é diária: recarregar não muda as recomendações.
        var again = await GetDashboardAsync(client);
        Assert.Equal(dashboard.Recommendations.Select(c => c.Id), again.Recommendations.Select(c => c.Id));
    }

    [SkippableFact]
    public async Task Dashboard_ReflectsDiscoveriesFavoritesAndQuizzes()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var zuko = await CharacterIdAsync("Zuko");
        var toph = await CharacterIdAsync("Toph Beifong");
        var quizzes = await api.GetJsonAsync<List<QuizListItemDto>>("/api/quizzes");

        (await client.PostAsync($"/api/characters/{zuko}/discovery", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/characters/{toph}/favorite", null)).EnsureSuccessStatusCode();

        // Primeiro quiz concluído com um erro: o destaque passa para o próximo não concluído.
        await using (var db = api.Database.CreateContext())
        {
            var questions = await db.Questions
                .Where(q => q.QuizId == quizzes[0].Id)
                .Include(q => q.Options)
                .OrderBy(q => q.Order)
                .ToListAsync();
            var answers = questions.Select((q, i) => new QuizAnswerRequest(
                q.Id, q.Options.First(o => o.IsCorrect == (i > 0)).Id));
            var submit = await client.PostAsJsonAsync(
                $"/api/quizzes/{quizzes[0].Id}/attempts", new SubmitQuizAttemptRequest([.. answers]));
            submit.EnsureSuccessStatusCode();
        }

        var dashboard = await GetDashboardAsync(client);

        // 10 (cadastro) + 10 (Zuko) + 20 (quiz) + conquistas que a descoberta/quiz possam ter liberado.
        Assert.True(dashboard.User.ExperiencePoints >= 40);
        Assert.Equal(DiscoveryKind.Character, dashboard.RecentDiscoveries[0].Kind);
        Assert.Equal(zuko, dashboard.RecentDiscoveries[0].Id);
        Assert.DoesNotContain(dashboard.Recommendations, c => c.Id == zuko);
        var favorite = Assert.Single(dashboard.Favorites);
        Assert.Equal(toph, favorite.Id);
        Assert.True(favorite.IsFavorite);
        Assert.Equal(quizzes[1].Id, dashboard.FeaturedQuiz!.Id);
        Assert.Equal((1, 1), (dashboard.Statistics.DiscoveredCharacters, dashboard.Statistics.CompletedQuizzes));
    }
}
