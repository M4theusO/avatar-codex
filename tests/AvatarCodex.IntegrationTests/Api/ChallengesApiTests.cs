using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ChallengesApiCollection.Name)]
public class ChallengesApiTests(ChallengesApiFixture api)
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

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, $"{(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return (await response.Content.ReadFromJsonAsync<T>(ContentApiFixture.JsonOptions))!;
    }

    /// <summary>Respostas perfeitas, com o gabarito lido direto do banco.</summary>
    private async Task<SubmitQuizAttemptRequest> PerfectAnswersAsync(int quizId)
    {
        await using var db = api.Database.CreateContext();
        var questions = await db.Questions.Where(q => q.QuizId == quizId).Include(q => q.Options).ToListAsync();
        return new([.. questions.Select(q => new QuizAnswerRequest(q.Id, q.Options.Single(o => o.IsCorrect).Id))]);
    }

    [SkippableFact]
    public async Task Challenges_RequireLogin()
    {
        api.Database.SkipIfUnavailable();
        var anonymous = api.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/challenges/daily")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/users/me/challenges")).StatusCode);
    }

    [SkippableFact]
    public async Task Daily_ReturnsThreeChallengesOfDifferentTypesForTodayInUtc()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();

        var daily = await ReadAsync<DailyChallengesDto>(await client.GetAsync("/api/challenges/daily"));
        var again = await ReadAsync<DailyChallengesDto>(await client.GetAsync("/api/challenges/daily"));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.Equal(today, daily.Date);
        Assert.Equal(new DateTimeOffset(today.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero), daily.ResetsAt);
        Assert.Equal(3, daily.Challenges.Count);
        Assert.Equal(3, daily.Challenges.Select(c => c.Type).Distinct().Count());
        Assert.All(daily.Challenges, c => Assert.False(c.IsCompleted));
        Assert.All(daily.Challenges, c => Assert.Equal(0, c.Progress));
        Assert.Equal(daily.Challenges.Select(c => c.Id), again.Challenges.Select(c => c.Id));

        // O painel traz os mesmos desafios do dia.
        var dashboard = await ReadAsync<DashboardDto>(await client.GetAsync("/api/users/me/dashboard"));
        Assert.Equal(daily.Challenges.Select(c => c.Id), dashboard.DailyChallenges.Challenges.Select(c => c.Id));
    }

    [SkippableFact]
    public async Task Activities_CompleteEveryChallengeOnceAndReturnItInTheReward()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var daily = await ReadAsync<DailyChallengesDto>(await client.GetAsync("/api/challenges/daily"));
        var characters = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?pageSize=5");
        var nations = await api.GetJsonAsync<List<NationDto>>("/api/nations");
        var quizzes = await api.GetJsonAsync<List<QuizListItemDto>>("/api/quizzes");

        // Descobertas em paralelo: a conclusão de cada desafio continua única.
        var discoveries = await Task.WhenAll(
            characters.Items.Select(c => client.PostAsync($"/api/characters/{c.Id}/discovery", null))
                .Concat(nations.Take(2).Select(n => client.PostAsync($"/api/nations/{n.Id}/discovery", null))));
        var rewards = (await Task.WhenAll(discoveries.Select(ReadAsync<DiscoveryResultDto>)))
            .Select(r => r.Reward)
            .ToList();
        foreach (var quiz in quizzes.Take(2))
        {
            var result = await ReadAsync<QuizAttemptResultDto>(
                await client.PostAsJsonAsync($"/api/quizzes/{quiz.Id}/attempts", await PerfectAnswersAsync(quiz.Id)));
            rewards.Add(result.Reward);
        }

        // Cada desafio do dia aparece uma única vez nos rewards dos eventos que o concluíram.
        var completedIds = rewards.Where(r => r is not null).SelectMany(r => r!.ChallengesCompleted).Select(c => c.Id).ToList();
        Assert.Equal(daily.Challenges.Select(c => c.Id).Order(), completedIds.Order());

        // Refazer um quiz não conclui nada de novo.
        var retry = await ReadAsync<QuizAttemptResultDto>(
            await client.PostAsJsonAsync($"/api/quizzes/{quizzes[0].Id}/attempts", await PerfectAnswersAsync(quizzes[0].Id)));
        Assert.True(retry.Reward is null || retry.Reward.ChallengesCompleted.Count == 0);

        var after = await ReadAsync<DailyChallengesDto>(await client.GetAsync("/api/challenges/daily"));
        Assert.All(after.Challenges, c =>
        {
            Assert.True(c.IsCompleted);
            Assert.Equal(c.Target, c.Progress);
            Assert.NotNull(c.CompletedAt);
        });

        var history = await ReadAsync<PagedResult<CompletedChallengeDto>>(await client.GetAsync("/api/users/me/challenges"));
        Assert.Equal(3, history.TotalItems);
        Assert.All(history.Items, c => Assert.Equal(daily.Date, c.Date));

        await using var db = api.Database.CreateContext();
        var userId = auth.User.Id;
        var challengeXp = await db.ExperienceTransactions
            .Where(t => t.UserId == userId && t.Type == ExperienceType.ChallengeCompleted)
            .ToListAsync();
        Assert.Equal(3, challengeXp.Count);
        Assert.Equal(daily.Challenges.Sum(c => c.ExperienceReward), challengeXp.Sum(t => t.Amount));
        Assert.Equal(3, await db.UserDailyChallenges.CountAsync(u => u.UserId == userId));
    }

    [SkippableFact]
    public async Task Generation_SimultaneousForTheSameDay_CreatesTheChallengesOnce()
    {
        api.Database.SkipIfUnavailable();
        // Uma data só deste teste: os desafios de hoje já foram gerados pelos outros testes.
        var day = new DateOnly(2099, 1, 1).AddDays(Random.Shared.Next(0, 3650));
        List<int> templateIds;
        await using (var db = api.Database.CreateContext())
        {
            var templates = await new ChallengeRepository(db).GetActiveTemplatesAsync(CancellationToken.None);
            templateIds = [.. ChallengeRules.PickForDay(templates, day).Select(t => t.Id)];
        }

        await Task.WhenAll(Enumerable.Range(0, 5).Select(async _ =>
        {
            await using var db = api.Database.CreateContext();
            await new ChallengeRepository(db).AddForDateAsync(day, templateIds, CancellationToken.None);
        }));

        await using var check = api.Database.CreateContext();
        Assert.Equal(3, await check.DailyChallenges.CountAsync(d => d.Date == day));
    }
}
