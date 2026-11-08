using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class ElementTestApiTests(ContentApiFixture api)
{
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

    /// <summary>Respostas escolhendo sempre a opção do elemento indicado (o mapa vem do banco, não da API).</summary>
    private async Task<List<ElementTestAnswerRequest>> AnswersForAsync(string element)
    {
        await using var db = api.Database.CreateContext();
        return await db.ElementTestOptions
            .Where(o => o.Element.Name == element)
            .Select(o => new ElementTestAnswerRequest(o.QuestionId, o.Id))
            .ToListAsync();
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task Get_RequiresLogin_AndNeverRevealsTheElementOfEachOption()
    {
        api.Database.SkipIfUnavailable();
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.CreateBrowserClient().GetAsync("/api/element-test")).StatusCode);
        var (client, _) = await NewUserClientAsync();

        var response = await client.GetAsync("/api/element-test");
        var body = await response.Content.ReadAsStringAsync();
        var test = await ReadAsync<ElementTestDto>(response);

        Assert.Equal(8, test.Questions.Count);
        Assert.All(test.Questions, q => Assert.Equal(4, q.Options.Count));
        Assert.Equal(ExperienceRewards.ElementTestCompleted, test.ExperienceReward);
        Assert.DoesNotContain("element", body.Replace("experienceReward", ""), StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task Submit_SavesTheElement_GrantsXpOnce_AndRetakingReplacesIt()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth) = await NewUserClientAsync();

        var first = await ReadAsync<ElementTestResultDto>(await client.PostAsJsonAsync(
            "/api/element-test/submissions", new SubmitElementTestRequest(await AnswersForAsync("Fogo"))));

        Assert.Equal("Fogo", first.Element.Name);
        Assert.NotEmpty(first.Element.Description);
        Assert.Equal(["Água", "Terra", "Fogo", "Ar"], first.Scores.Select(s => s.Element.Name));
        Assert.Equal([0, 0, 8, 0], first.Scores.Select(s => s.Points));
        Assert.Equal(ExperienceRewards.ElementTestCompleted, first.ExperienceEarned);
        Assert.Equal(ExperienceRewards.ElementTestCompleted, first.Reward!.XpGained);
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);
        Assert.Equal("Fogo", me!.Element?.Name);

        var retake = await ReadAsync<ElementTestResultDto>(await client.PostAsJsonAsync(
            "/api/element-test/submissions", new SubmitElementTestRequest(await AnswersForAsync("Água"))));

        Assert.Equal("Água", retake.Element.Name);
        Assert.Null(retake.Reward);
        Assert.Equal(0, retake.ExperienceEarned);
        me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);
        Assert.Equal("Água", me!.Element?.Name);
        Assert.Equal(first.Reward.TotalXp, me.ExperiencePoints);

        await using var db = api.Database.CreateContext();
        Assert.Equal(1, await db.ExperienceTransactions.CountAsync(
            t => t.UserId == auth.User.Id && t.Type == ExperienceType.ElementTestCompleted));
    }

    [SkippableFact]
    public async Task Submit_InvalidAnswers_ReturnsBadRequestWithoutChangingTheProfile()
    {
        api.Database.SkipIfUnavailable();
        var (client, _) = await NewUserClientAsync();
        var fire = await AnswersForAsync("Fogo");
        var air = await AnswersForAsync("Ar");

        // Falta uma pergunta; outra traz a opção de uma pergunta diferente.
        var invalid = fire.Skip(1).ToList();
        invalid[0] = invalid[0] with { OptionId = air.Single(a => a.QuestionId != invalid[0].QuestionId && a.QuestionId == fire[2].QuestionId).OptionId };
        var response = await client.PostAsJsonAsync("/api/element-test/submissions", new SubmitElementTestRequest(invalid));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Falta responder 1 pergunta.", body);
        Assert.Contains("não pertence à pergunta", body);
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);
        Assert.Null(me!.Element);
    }
}
