using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class QuizApiTests(ContentApiFixture api)
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

    private async Task<int> QuizIdAsync(string title) =>
        (await api.GetJsonAsync<List<QuizListItemDto>>("/api/quizzes")).Single(q => q.Title == title).Id;

    /// <summary>Gabarito lido direto do banco: a API nunca o expõe antes da submissão.</summary>
    private async Task<List<(int QuestionId, int CorrectId, int WrongId)>> AnswerKeyAsync(int quizId)
    {
        await using var db = api.Database.CreateContext();
        var questions = await db.Questions
            .Where(q => q.QuizId == quizId)
            .Include(q => q.Options)
            .OrderBy(q => q.Order)
            .ToListAsync();
        return [.. questions.Select(q => (q.Id, q.Options.Single(o => o.IsCorrect).Id, q.Options.First(o => !o.IsCorrect).Id))];
    }

    private static SubmitQuizAttemptRequest Answers(
        List<(int QuestionId, int CorrectId, int WrongId)> key, int wrongAnswers = 0) =>
        new([.. key.Select((k, i) => new QuizAnswerRequest(k.QuestionId, i < wrongAnswers ? k.WrongId : k.CorrectId))]);

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<T>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task List_IsPublicAndFiltersByCategoryAndDifficulty()
    {
        api.Database.SkipIfUnavailable();

        var all = await api.GetJsonAsync<List<QuizListItemDto>>("/api/quizzes");
        var hard = await api.GetJsonAsync<List<QuizListItemDto>>("/api/quizzes?difficulty=hard");
        var characters = await api.GetJsonAsync<List<QuizListItemDto>>("/api/quizzes?category=personagens");

        Assert.Equal(4, all.Count);
        Assert.All(all, q => Assert.Equal(10, q.QuestionCount));
        Assert.All(all, q => Assert.Null(q.BestResult));
        Assert.Equal(QuizDifficulty.Easy, all[0].Difficulty);
        Assert.Equal(["A Guerra dos Cem Anos"], hard.Select(q => q.Title));
        Assert.Equal(["O Time Avatar"], characters.Select(q => q.Title));

        var invalid = await api.CreateClient().GetAsync("/api/quizzes?difficulty=impossible");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [SkippableFact]
    public async Task GetForPlay_RequiresLoginAndNeverExposesAnswers()
    {
        api.Database.SkipIfUnavailable();
        var quizId = await QuizIdAsync("O Time Avatar");

        var anonymous = await api.CreateClient().GetAsync($"/api/quizzes/{quizId}");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var (client, _) = await NewUserClientAsync();
        var response = await client.GetAsync($"/api/quizzes/{quizId}");
        var json = await response.Content.ReadAsStringAsync();

        // RN-09: nem a resposta certa nem a explicação saem antes da submissão.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("isCorrect", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanation", json, StringComparison.OrdinalIgnoreCase);

        var quiz = (await response.Content.ReadFromJsonAsync<QuizDto>(ContentApiFixture.JsonOptions))!;
        Assert.Equal(10, quiz.Questions.Count);
        Assert.All(quiz.Questions, q => Assert.Equal(4, q.Options.Count));
        Assert.Equal("Qual é o nome do bisão voador de Aang?", quiz.Questions[0].Text);

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/quizzes/999999")).StatusCode);
    }

    [SkippableFact]
    public async Task Submit_GrantsXpOnlyTheFirstTimeAndRecordsEveryAttempt()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var quizId = await QuizIdAsync("Os Quatro Elementos");
        var key = await AnswerKeyAsync(quizId);

        var first = await ReadAsync<QuizAttemptResultDto>(
            await client.PostAsJsonAsync($"/api/quizzes/{quizId}/attempts", Answers(key, wrongAnswers: 2)), HttpStatusCode.Created);
        var perfect = await ReadAsync<QuizAttemptResultDto>(
            await client.PostAsJsonAsync($"/api/quizzes/{quizId}/attempts", Answers(key)), HttpStatusCode.Created);
        var again = await ReadAsync<QuizAttemptResultDto>(
            await client.PostAsJsonAsync($"/api/quizzes/{quizId}/attempts", Answers(key)), HttpStatusCode.Created);

        Assert.Equal((8, 10, 80, 20), (first.Score, first.TotalQuestions, first.Percentage, first.ExperienceEarned));
        Assert.False(first.Results[0].IsCorrect);
        Assert.Equal(key[0].CorrectId, first.Results[0].CorrectOptionId);
        Assert.False(string.IsNullOrWhiteSpace(first.Results[0].Explanation));

        Assert.Equal((10, 100, 50), (perfect.Score, perfect.Percentage, perfect.ExperienceEarned));
        Assert.Equal((0, null), (again.ExperienceEarned, again.Reward));

        var history = await client.GetFromJsonAsync<PagedResult<QuizAttemptDto>>(
            "/api/users/me/quiz-history", ContentApiFixture.JsonOptions);
        Assert.Equal(3, history!.TotalItems);
        Assert.Equal([0, 50, 20], history.Items.Select(a => a.ExperienceEarned));
        Assert.All(history.Items, a => Assert.Equal("Os Quatro Elementos", a.QuizTitle));

        var list = await client.GetFromJsonAsync<List<QuizListItemDto>>("/api/quizzes", ContentApiFixture.JsonOptions);
        Assert.Equal(new QuizBestResultDto(10, 10, 100, 3), list!.Single(q => q.Id == quizId).BestResult);

        var stats = await client.GetFromJsonAsync<UserStatisticsDto>("/api/users/me/statistics", ContentApiFixture.JsonOptions);
        Assert.Equal((1, 4, 3, 93), (stats!.CompletedQuizzes, stats.TotalQuizzes, stats.QuizAttempts, stats.AccuracyPercent));

        // 10 (Primeiro passo) + 20 + 50, mais os desafios de quiz do dia que essas tentativas cumpriram.
        var challengeXp = new[] { first, perfect, again }
            .Where(r => r.Reward is not null)
            .Sum(r => r.Reward!.ChallengesCompleted.Sum(c => c.ExperienceReward));
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);
        Assert.Equal(10 + 20 + 50 + challengeXp, me!.ExperiencePoints);
        _ = auth;
    }

    [SkippableFact]
    public async Task Submit_ConcurrentFirstAttempts_GrantXpOnce()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var quizId = await QuizIdAsync("A Lenda de Korra");
        var key = await AnswerKeyAsync(quizId);

        var responses = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => client.PostAsJsonAsync($"/api/quizzes/{quizId}/attempts", Answers(key))));
        Assert.All(responses, r => Assert.Equal(HttpStatusCode.Created, r.StatusCode));

        await using var db = api.Database.CreateContext();
        var userId = auth.User.Id;
        Assert.Equal(5, await db.QuizAttempts.CountAsync(a => a.UserId == userId));
        Assert.Equal(70, await db.QuizAttempts.Where(a => a.UserId == userId).SumAsync(a => a.ExperienceEarned));
        var challengeXp = await db.ExperienceTransactions
            .Where(t => t.UserId == userId && t.Type == ExperienceType.ChallengeCompleted)
            .SumAsync(t => t.Amount);
        Assert.Equal(10 + 70 + challengeXp, await db.Users.Where(u => u.Id == userId).Select(u => u.ExperiencePoints).SingleAsync());
    }

    [SkippableFact]
    public async Task Submit_InvalidAnswers_ReturnsBadRequestWithoutRecording()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();
        var quizId = await QuizIdAsync("O Time Avatar");
        var key = await AnswerKeyAsync(quizId);
        var otherKey = await AnswerKeyAsync(await QuizIdAsync("A Lenda de Korra"));

        var missing = new SubmitQuizAttemptRequest([.. Answers(key).Answers!.Skip(1)]);
        var foreignOption = new SubmitQuizAttemptRequest(
            [new QuizAnswerRequest(key[0].QuestionId, key[1].CorrectId), .. Answers(key).Answers!.Skip(1)]);
        var foreignQuestion = new SubmitQuizAttemptRequest(
            [.. Answers(key).Answers!, new QuizAnswerRequest(otherKey[0].QuestionId, otherKey[0].CorrectId)]);

        foreach (var request in new[] { missing, foreignOption, foreignQuestion, new SubmitQuizAttemptRequest(null) })
        {
            var response = await client.PostAsJsonAsync($"/api/quizzes/{quizId}/attempts", request);
            var error = await ReadAsync<ErrorBody>(response, HttpStatusCode.BadRequest);
            Assert.Contains(error.Errors, e => e.Field == "answers");
        }

        Assert.Equal(HttpStatusCode.NotFound,
            (await client.PostAsJsonAsync("/api/quizzes/999999/attempts", Answers(key))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await api.CreateClient().PostAsJsonAsync($"/api/quizzes/{quizId}/attempts", Answers(key))).StatusCode);

        await using var db = api.Database.CreateContext();
        Assert.Equal(0, await db.QuizAttempts.CountAsync(a => a.UserId == auth.User.Id));
    }

    [SkippableFact]
    public async Task QuizAchievements_UnlockFromAnswersAndPerfectScores()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var quizId = await QuizIdAsync("O Time Avatar");
        var key = await AnswerKeyAsync(quizId);

        // 100 respostas corretas = 10 tentativas perfeitas (refazer conta para essa conquista).
        var results = new List<QuizAttemptResultDto>();
        for (var i = 0; i < 10; i++)
        {
            results.Add(await ReadAsync<QuizAttemptResultDto>(
                await client.PostAsJsonAsync($"/api/quizzes/{quizId}/attempts", Answers(key)), HttpStatusCode.Created));
        }

        Assert.All(results.Take(9).Skip(1), r => Assert.Null(r.Reward));
        Assert.Equal("Conhecedor de Avatar", Assert.Single(results[^1].Reward!.AchievementsUnlocked).Name);
        Assert.Equal(0, results[^1].ExperienceEarned);

        var achievements = await client.GetFromJsonAsync<List<UserAchievementDto>>(
            "/api/users/me/achievements", ContentApiFixture.JsonOptions);
        Assert.True(achievements!.Single(a => a.Name == "Conhecedor de Avatar").IsUnlocked);
    }

    private record ErrorBody(int Status, string Message, List<FieldErrorBody> Errors);

    private record FieldErrorBody(string Field, string Message);
}
