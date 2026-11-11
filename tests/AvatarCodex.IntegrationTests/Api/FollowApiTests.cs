using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;

namespace AvatarCodex.IntegrationTests.Api;

/// <summary>Seguir usuários (Fase 24): PUT/DELETE /api/users/{username}/follow, listas e ranking "Seguindo".</summary>
[Collection(ContentApiCollection.Name)]
public class FollowApiTests(ContentApiFixture api)
{
    private record TestUser(HttpClient Client, string Username);

    private async Task<TestUser> NewUserAsync(string prefix = "fol")
    {
        var client = api.CreateBrowserClient();
        var username = $"{prefix}_{Guid.NewGuid().ToString("N")[..8]}";
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(username, $"{username}@example.com", "Senha123", "Senha123"));
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return new TestUser(client, username);
    }

    private static Task<HttpResponseMessage> FollowAsync(TestUser who, string username) =>
        who.Client.PutAsync($"/api/users/{username}/follow", null);

    private static Task<HttpResponseMessage> UnfollowAsync(TestUser who, string username) =>
        who.Client.DeleteAsync($"/api/users/{username}/follow");

    private static async Task<T> GetAsync<T>(TestUser who, string url)
    {
        var response = await who.Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task Follow_ShowsInProfileAndLists_IsIdempotent_AndUnfollowUndoes()
    {
        api.Database.SkipIfUnavailable();
        var fan = await NewUserAsync();
        var star = await NewUserAsync();

        Assert.Equal(HttpStatusCode.NoContent, (await FollowAsync(fan, star.Username)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await FollowAsync(fan, star.Username.ToUpperInvariant())).StatusCode);

        var seen = await GetAsync<PublicProfileDto>(fan, $"/api/users/{star.Username}");
        Assert.True(seen.IsFollowing);
        Assert.Equal(1, seen.FollowersCount);
        Assert.Equal(0, seen.FollowingCount);

        var own = await GetAsync<PublicProfileDto>(fan, $"/api/users/{fan.Username}");
        Assert.False(own.IsFollowing);
        Assert.Equal(1, own.FollowingCount);

        var following = await GetAsync<PagedResult<FollowUserDto>>(fan, "/api/users/me/following");
        Assert.Equal(star.Username, Assert.Single(following.Items).Username);
        var followers = await GetAsync<PagedResult<FollowUserDto>>(star, "/api/users/me/followers");
        Assert.Equal(fan.Username, Assert.Single(followers.Items).Username);

        Assert.Equal(HttpStatusCode.NoContent, (await UnfollowAsync(fan, star.Username)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await UnfollowAsync(fan, star.Username)).StatusCode);
        var after = await GetAsync<PublicProfileDto>(fan, $"/api/users/{star.Username}");
        Assert.False(after.IsFollowing);
        Assert.Equal(0, after.FollowersCount);
        Assert.Empty((await GetAsync<PagedResult<FollowUserDto>>(fan, "/api/users/me/following")).Items);
    }

    [SkippableFact]
    public async Task Follow_Refused_ForPrivateMissingSelfAndDemo()
    {
        api.Database.SkipIfUnavailable();
        var fan = await NewUserAsync();
        var hidden = await NewUserAsync();
        (await hidden.Client.PutAsJsonAsync("/api/users/me/privacy", new UpdatePrivacyRequest(true))).EnsureSuccessStatusCode();

        var privateResponse = await FollowAsync(fan, hidden.Username);
        var missingResponse = await FollowAsync(fan, $"ninguem_{Guid.NewGuid().ToString("N")[..8]}");
        Assert.Equal(HttpStatusCode.NotFound, privateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingResponse.StatusCode);
        Assert.Equal(await missingResponse.Content.ReadAsStringAsync(), await privateResponse.Content.ReadAsStringAsync());

        var self = await FollowAsync(fan, fan.Username);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, self.StatusCode);
        Assert.Contains(FollowRules.SelfMessage, await self.Content.ReadAsStringAsync());

        var demo = await FollowAsync(fan, DemoAccount.Username);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, demo.StatusCode);
        Assert.Contains(FollowRules.DemoMessage, await demo.Content.ReadAsStringAsync());

        // Deixar de seguir nunca falha, nem para quem não existe.
        Assert.Equal(HttpStatusCode.NoContent, (await UnfollowAsync(fan, $"ninguem_{Guid.NewGuid().ToString("N")[..8]}")).StatusCode);
        Assert.Empty((await GetAsync<PagedResult<FollowUserDto>>(fan, "/api/users/me/following")).Items);
    }

    [SkippableFact]
    public async Task FollowedThenPrivate_StaysInTheList_ProfileHidden_UnfollowStillWorks()
    {
        api.Database.SkipIfUnavailable();
        var fan = await NewUserAsync();
        var star = await NewUserAsync();
        (await FollowAsync(fan, star.Username)).EnsureSuccessStatusCode();

        (await star.Client.PutAsJsonAsync("/api/users/me/privacy", new UpdatePrivacyRequest(true))).EnsureSuccessStatusCode();

        var entry = Assert.Single((await GetAsync<PagedResult<FollowUserDto>>(fan, "/api/users/me/following")).Items);
        Assert.True(entry.IsProfilePrivate);
        Assert.Equal(HttpStatusCode.NotFound, (await fan.Client.GetAsync($"/api/users/{star.Username}")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await UnfollowAsync(fan, star.Username)).StatusCode);
        Assert.Empty((await GetAsync<PagedResult<FollowUserDto>>(fan, "/api/users/me/following")).Items);
    }

    [SkippableFact]
    public async Task Ranking_Following_ListsOnlyTheViewerAndWhoTheyFollow()
    {
        api.Database.SkipIfUnavailable();
        var fan = await NewUserAsync();
        var star = await NewUserAsync();
        var stranger = await NewUserAsync();
        (await FollowAsync(fan, star.Username)).EnsureSuccessStatusCode();

        var ranking = await GetAsync<RankingDto>(fan, "/api/ranking?following=true&period=month");

        Assert.True(ranking.Following);
        Assert.Equal(2, ranking.Entries.TotalItems);
        Assert.Equal(
            new[] { fan.Username, star.Username }.Order(),
            ranking.Entries.Items.Select(e => e.Username).Order());
        Assert.DoesNotContain(ranking.Entries.Items, e => e.Username == stranger.Username);
        Assert.NotNull(ranking.CurrentUser);
    }

    [SkippableFact]
    public async Task DeletingTheAccount_RemovesItsFollows()
    {
        api.Database.SkipIfUnavailable();
        var fan = await NewUserAsync();
        var star = await NewUserAsync();
        (await FollowAsync(fan, star.Username)).EnsureSuccessStatusCode();

        var delete = await fan.Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/users/me")
        {
            Content = JsonContent.Create(new DeleteAccountRequest("Senha123"))
        });
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        Assert.Equal(0, (await GetAsync<PublicProfileDto>(star, $"/api/users/{star.Username}")).FollowersCount);
        Assert.Empty((await GetAsync<PagedResult<FollowUserDto>>(star, "/api/users/me/followers")).Items);
    }

    [SkippableFact]
    public async Task WithoutToken_ReturnsUnauthorized()
    {
        api.Database.SkipIfUnavailable();
        var anonymous = api.CreateBrowserClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PutAsync($"/api/users/{DemoAccount.Username}/follow", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/users/me/following")).StatusCode);
    }
}
