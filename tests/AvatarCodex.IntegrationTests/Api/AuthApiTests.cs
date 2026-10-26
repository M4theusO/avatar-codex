using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Infrastructure.Data.Seed;

namespace AvatarCodex.IntegrationTests.Api;

[Collection(ContentApiCollection.Name)]
public class AuthApiTests(ContentApiFixture api)
{
    private static RegisterRequest NewRegistration()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new RegisterRequest($"user_{suffix}", $"user_{suffix}@example.com", "Senha123", "Senha123");
    }

    private static async Task<AuthResponse> ReadAuthAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<AuthResponse>(ContentApiFixture.JsonOptions))!;

    private static string? RefreshCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(v => v.StartsWith("refresh_token="))
            : null;

    [SkippableFact]
    public async Task Register_CreatesAccountStartsSessionAndSetsSecureCookie()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateBrowserClient();
        var registration = NewRegistration();

        var response = await client.PostAsJsonAsync("/api/auth/register", registration);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await ReadAuthAsync(response);
        Assert.False(string.IsNullOrEmpty(auth.AccessToken));
        Assert.Equal(900, auth.ExpiresIn);
        Assert.Equal(registration.Username, auth.User.Username);
        Assert.Equal(1, auth.User.Level);

        var cookie = RefreshCookie(response)!.ToLowerInvariant();
        Assert.Contains("httponly", cookie);
        Assert.Contains("secure", cookie);
        Assert.Contains("samesite=strict", cookie);
        Assert.Contains("path=/api/auth", cookie);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("refresh", body, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task Register_DuplicateEmailIgnoringCase_ReturnsConflict()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateBrowserClient();
        var registration = NewRegistration();
        await client.PostAsJsonAsync("/api/auth/register", registration);

        var response = await client.PostAsJsonAsync("/api/auth/register",
            NewRegistration() with { Email = registration.Email.ToUpperInvariant() });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [SkippableFact]
    public async Task Register_InvalidData_ReturnsFieldErrors()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateBrowserClient().PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("x", "invalido", "curta", "outra"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"field\":\"username\"", body);
        Assert.Contains("\"field\":\"email\"", body);
        Assert.Contains("\"field\":\"password\"", body);
        Assert.Contains("\"field\":\"confirmPassword\"", body);
    }

    [SkippableFact]
    public async Task Login_DemoUser_ReturnsTokenThatAuthorizesMe()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateBrowserClient();

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(DemoUserSeeder.Email, DemoUserSeeder.Password));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var auth = await ReadAuthAsync(login);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);

        Assert.Equal(DemoUserSeeder.Username, me!.Username);
    }

    [SkippableFact]
    public async Task Login_WrongPassword_ReturnsUnauthorizedWithStandardError()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateBrowserClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(DemoUserSeeder.Email, "SenhaErrada1"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Email ou senha inválidos.", await response.Content.ReadAsStringAsync());
        Assert.Null(RefreshCookie(response));
    }

    [SkippableTheory]
    [InlineData(null)]
    [InlineData("token.invalido.qualquer")]
    public async Task Me_WithoutValidToken_ReturnsUnauthorizedWithStandardError(string? token)
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateBrowserClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("\"status\":401", await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task Refresh_WithCookie_RotatesAndOldCookieStopsWorking()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateBrowserClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", NewRegistration());
        var firstCookie = RefreshCookie(register)!.Split(';')[0];

        var refresh = await client.PostAsync("/api/auth/refresh", null);
        Assert.Equal(HttpStatusCode.OK, refresh.StatusCode);
        Assert.False(string.IsNullOrEmpty((await ReadAuthAsync(refresh)).AccessToken));
        Assert.NotEqual(firstCookie, RefreshCookie(refresh)!.Split(';')[0]);

        // Outro "navegador" reapresentando o cookie antigo, já rotacionado.
        var stale = api.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false });
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", firstCookie);
        var reuse = await stale.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
        Assert.Contains("expires=", RefreshCookie(reuse)!, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task Refresh_WithoutCookie_ReturnsUnauthorized()
    {
        api.Database.SkipIfUnavailable();

        var response = await api.CreateBrowserClient().PostAsync("/api/auth/refresh", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task Logout_RevokesSessionAndClearsCookie()
    {
        api.Database.SkipIfUnavailable();
        var client = api.CreateBrowserClient();
        var register = await client.PostAsJsonAsync("/api/auth/register", NewRegistration());
        var cookie = RefreshCookie(register)!.Split(';')[0];

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Contains("expires=", RefreshCookie(logout)!, StringComparison.OrdinalIgnoreCase);

        // Mesmo que alguém ainda tenha o cookie, a sessão foi revogada no banco.
        var stale = api.CreateClient(new() { BaseAddress = new Uri("https://localhost"), HandleCookies = false });
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", cookie);
        Assert.Equal(HttpStatusCode.Unauthorized, (await stale.SendAsync(request)).StatusCode);
    }
}
