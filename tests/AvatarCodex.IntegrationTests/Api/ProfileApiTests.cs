using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;

namespace AvatarCodex.IntegrationTests.Api;

/// <summary>Perfil público (GET /api/users/{username}) e visibilidade (PUT /api/users/me/privacy).</summary>
[Collection(ContentApiCollection.Name)]
public class ProfileApiTests(ContentApiFixture api)
{
    private record TestUser(HttpClient Client, Guid Id, string Username);

    private async Task<TestUser> NewUserAsync(string prefix = "prof")
    {
        var client = api.CreateBrowserClient();
        var username = $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(username, $"{username}@example.com", "Senha123", "Senha123"));
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return new TestUser(client, auth.User.Id, username);
    }

    private static Task<HttpResponseMessage> SetPrivacyAsync(HttpClient client, bool? isPrivate) =>
        client.PutAsJsonAsync("/api/users/me/privacy", new UpdatePrivacyRequest(isPrivate));

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task Get_PublicProfile_ShowsGameDataOnly()
    {
        api.Database.SkipIfUnavailable();
        var owner = await NewUserAsync();
        var viewer = await NewUserAsync();

        var response = await viewer.Client.GetAsync($"/api/users/{owner.Username}");
        var body = await response.Content.ReadAsStringAsync();
        var profile = await ReadAsync<PublicProfileDto>(response);

        Assert.Equal(owner.Username, profile.Username);
        Assert.False(profile.IsCurrentUser);
        Assert.False(profile.IsProfilePrivate);
        // "Primeiro passo" do cadastro: 10 XP e uma conquista.
        Assert.Equal(10, profile.ExperiencePoints);
        Assert.Equal(LevelCalculator.GetLevel(10), profile.Level);
        var achievement = Assert.Single(profile.Achievements);
        Assert.Equal("Primeiro passo", achievement.Name);
        Assert.Equal(1, profile.Statistics.UnlockedAchievements);
        Assert.True(profile.Statistics.TotalCharacters > 0);

        Assert.DoesNotContain("@example.com", body);
        Assert.DoesNotContain(owner.Id.ToString(), body);
    }

    [SkippableFact]
    public async Task Get_IgnoresCase_AndMarksTheOwner()
    {
        api.Database.SkipIfUnavailable();
        var owner = await NewUserAsync();

        var profile = await ReadAsync<PublicProfileDto>(await owner.Client.GetAsync($"/api/users/{owner.Username.ToUpperInvariant()}"));

        Assert.Equal(owner.Username, profile.Username);
        Assert.True(profile.IsCurrentUser);
    }

    [SkippableFact]
    public async Task Private_IsNotFoundForOthers_VisibleToTheOwner_AndFlaggedInTheRanking()
    {
        api.Database.SkipIfUnavailable();
        var owner = await NewUserAsync();
        var viewer = await NewUserAsync();

        var user = await ReadAsync<UserDto>(await SetPrivacyAsync(owner.Client, true));
        Assert.True(user.IsProfilePrivate);

        var hidden = await viewer.Client.GetAsync($"/api/users/{owner.Username}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
        // Mesma resposta de um nome que não existe: não revela a conta.
        var missing = await viewer.Client.GetAsync($"/api/users/ninguem_{Guid.NewGuid().ToString("N")[..8]}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal(await missing.Content.ReadAsStringAsync(), await hidden.Content.ReadAsStringAsync());

        var own = await ReadAsync<PublicProfileDto>(await owner.Client.GetAsync($"/api/users/{owner.Username}"));
        Assert.True(own.IsProfilePrivate);

        var ranking = await ReadAsync<RankingDto>(await viewer.Client.GetAsync("/api/ranking?pageSize=50&period=month"));
        var entry = ranking.Entries.Items.SingleOrDefault(e => e.Username == owner.Username);
        if (entry is not null)
            Assert.True(entry.IsProfilePrivate);

        await ReadAsync<UserDto>(await SetPrivacyAsync(owner.Client, false));
        Assert.Equal(HttpStatusCode.OK, (await viewer.Client.GetAsync($"/api/users/{owner.Username}")).StatusCode);
    }

    [SkippableFact]
    public async Task DeletedAccount_ProfileIsGone()
    {
        api.Database.SkipIfUnavailable();
        var owner = await NewUserAsync();
        var viewer = await NewUserAsync();

        var delete = await owner.Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/users/me")
        {
            Content = JsonContent.Create(new DeleteAccountRequest("Senha123"))
        });
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        Assert.Equal(HttpStatusCode.NotFound, (await viewer.Client.GetAsync($"/api/users/{owner.Username}")).StatusCode);
    }

    [SkippableFact]
    public async Task Privacy_MissingValue_ReturnsFieldError()
    {
        api.Database.SkipIfUnavailable();
        var user = await NewUserAsync();

        var response = await SetPrivacyAsync(user.Client, null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("isProfilePrivate", await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task Privacy_DemoAccount_IsRefused()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateBrowserClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(DemoAccount.Email, DemoAccount.Password));
        var auth = await ReadAsync<AuthResponse>(login);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var response = await SetPrivacyAsync(client, true);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(AccountService.DemoPrivacyMessage, await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task Get_WithoutToken_ReturnsUnauthorized()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateBrowserClient().GetAsync($"/api/users/{DemoAccount.Username}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
