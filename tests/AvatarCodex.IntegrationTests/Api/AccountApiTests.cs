using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class AccountApiTests(ContentApiFixture api)
{
    private async Task<(HttpClient Client, AuthResponse Auth, string RefreshCookie)> LoginAsync(
        string email, string password, bool register)
    {
        var client = api.CreateBrowserClient();
        if (register)
        {
            var username = email.Split('@')[0];
            (await client.PostAsJsonAsync("/api/auth/register",
                new RegisterRequest(username, email, password, password))).EnsureSuccessStatusCode();
        }

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        login.EnsureSuccessStatusCode();
        var auth = (await login.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;
        var cookie = login.Headers.GetValues("Set-Cookie").First(v => v.StartsWith("refresh_token=")).Split(';')[0];
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return (client, auth, cookie);
    }

    private static string NewEmail() => $"user_{Guid.NewGuid().ToString("N")[..8]}@example.com";

    private static Task<HttpResponseMessage> DeleteAccountAsync(HttpClient client, string password) =>
        client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/users/me")
        {
            Content = JsonContent.Create(new DeleteAccountRequest(password))
        });

    [SkippableFact]
    public async Task Delete_RightPassword_RemovesUserWithAllDataAndEndsTheSession()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, cookie) = await LoginAsync(NewEmail(), "Senha123", register: true);
        // Deixa dados em várias tabelas: descoberta + XP (o cadastro já gerou conquista e XP).
        var characters = await api.GetJsonAsync<PagedResult<CharacterListItemDto>>("/api/characters?search=Zuko");
        (await client.PostAsync($"/api/characters/{characters.Items.First().Id}/discovery", null)).EnsureSuccessStatusCode();
        (await client.PostAsync($"/api/characters/{characters.Items.First().Id}/favorite", null)).EnsureSuccessStatusCode();

        var response = await DeleteAccountAsync(client, "Senha123");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"),
            v => v.StartsWith("refresh_token=;") && v.Contains("expires=", StringComparison.OrdinalIgnoreCase));

        await using var db = api.Database.CreateContext();
        var userId = auth.User.Id;
        Assert.False(await db.Users.AnyAsync(u => u.Id == userId));
        Assert.False(await db.RefreshTokens.AnyAsync(t => t.UserId == userId));
        Assert.False(await db.CharacterDiscoveries.AnyAsync(d => d.UserId == userId));
        Assert.False(await db.FavoriteCharacters.AnyAsync(f => f.UserId == userId));
        Assert.False(await db.ExperienceTransactions.AnyAsync(t => t.UserId == userId));
        Assert.False(await db.UserAchievements.AnyAsync(a => a.UserId == userId));

        // O access token ainda não expirou, mas o usuário não existe mais; o refresh também acabou.
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
        var refresh = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refresh.Headers.Add("Cookie", cookie);
        var stale = api.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false });
        Assert.Equal(HttpStatusCode.Unauthorized, (await stale.SendAsync(refresh)).StatusCode);
    }

    [SkippableFact]
    public async Task Delete_WrongPassword_ReturnsFieldErrorAndKeepsTheUser()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, _) = await LoginAsync(NewEmail(), "Senha123", register: true);

        var response = await DeleteAccountAsync(client, "Errada123");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"field\":\"password\"", body);
        Assert.Contains(AccountService.WrongPasswordMessage, body);
        await using var db = api.Database.CreateContext();
        Assert.True(await db.Users.AnyAsync(u => u.Id == auth.User.Id));
    }

    [SkippableFact]
    public async Task Delete_DemoAccount_IsRefused()
    {
        api.Database.SkipIfUnavailable();
        var (client, _, _) = await LoginAsync(DemoAccount.Email, DemoAccount.Password, register: false);

        var response = await DeleteAccountAsync(client, DemoAccount.Password);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(AccountService.DemoAccountMessage, await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task Delete_WithoutToken_ReturnsUnauthorized()
    {
        api.Database.SkipIfUnavailable();

        var response = await DeleteAccountAsync(api.CreateBrowserClient(), "Senha123");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
