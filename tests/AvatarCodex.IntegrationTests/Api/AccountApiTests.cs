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

    // ---- Nome de usuário ----

    private static Task<HttpResponseMessage> UpdateProfileAsync(HttpClient client, string username) =>
        client.PatchAsJsonAsync("/api/users/me", new UpdateProfileRequest(username));

    [SkippableFact]
    public async Task UpdateProfile_NewUsername_IsSavedAndReturned()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, _) = await LoginAsync(NewEmail(), "Senha123", register: true);
        var newName = $"novo_{Guid.NewGuid().ToString("N")[..8]}";

        var response = await UpdateProfileAsync(client, newName);

        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserDto>(ContentApiFixture.JsonOptions);
        Assert.Equal(newName, user!.Username);
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me", ContentApiFixture.JsonOptions);
        Assert.Equal(newName, me!.Username);
        Assert.Equal(auth.User.Email, me.Email);
    }

    [SkippableFact]
    public async Task UpdateProfile_UsernameInUse_ReturnsConflict()
    {
        api.Database.SkipIfUnavailable();
        var other = NewEmail();
        await LoginAsync(other, "Senha123", register: true);
        var (client, _, _) = await LoginAsync(NewEmail(), "Senha123", register: true);

        var response = await UpdateProfileAsync(client, other.Split('@')[0].ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(AccountService.UsernameTakenMessage, await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task UpdateProfile_InvalidUsername_ReturnsFieldError()
    {
        api.Database.SkipIfUnavailable();
        var (client, _, _) = await LoginAsync(NewEmail(), "Senha123", register: true);

        var response = await UpdateProfileAsync(client, "a b");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("\"field\":\"username\"", await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task UpdateProfile_DemoAccount_IsRefused()
    {
        api.Database.SkipIfUnavailable();
        var (client, _, _) = await LoginAsync(DemoAccount.Email, DemoAccount.Password, register: false);

        var response = await UpdateProfileAsync(client, "outro_demo");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ---- Foto de perfil ----

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 5, 6, 7, 8];

    private static Task<HttpResponseMessage> UploadAvatarAsync(HttpClient client, byte[] data, string fileName = "foto.png")
    {
        var form = new MultipartFormDataContent { { new ByteArrayContent(data), "file", fileName } };
        return client.PutAsync("/api/users/me/avatar", form);
    }

    private static async Task<UserDto> ReadUserAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserDto>(ContentApiFixture.JsonOptions))!;
    }

    [SkippableFact]
    public async Task Avatar_Upload_IsServedPubliclyWithLongCache()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, _) = await LoginAsync(NewEmail(), "Senha123", register: true);

        var user = await ReadUserAsync(await UploadAvatarAsync(client, Png));

        Assert.StartsWith("/api/avatars/", user.AvatarUrl);
        Assert.DoesNotContain(auth.User.Id.ToString("N"), user.AvatarUrl);
        var image = await api.CreateBrowserClient().GetAsync(user.AvatarUrl);
        Assert.Equal(HttpStatusCode.OK, image.StatusCode);
        Assert.Equal("image/png", image.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Png, await image.Content.ReadAsByteArrayAsync());
        Assert.Contains("immutable", image.Headers.CacheControl!.ToString());
        Assert.Equal("nosniff", image.Headers.GetValues("X-Content-Type-Options").Single());
    }

    [SkippableFact]
    public async Task Avatar_SecondUpload_ReplacesTheFirstInsteadOfAddingAnother()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, _) = await LoginAsync(NewEmail(), "Senha123", register: true);
        var first = await ReadUserAsync(await UploadAvatarAsync(client, Png));

        var second = await ReadUserAsync(await UploadAvatarAsync(client, Jpeg, "outra.jpg"));

        Assert.NotEqual(first.AvatarUrl, second.AvatarUrl);
        var anonymous = api.CreateBrowserClient();
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync(first.AvatarUrl)).StatusCode);
        var image = await anonymous.GetAsync(second.AvatarUrl);
        Assert.Equal("image/jpeg", image.Content.Headers.ContentType!.MediaType);
        await using var db = api.Database.CreateContext();
        var stored = Assert.Single(await db.UserAvatars.Where(a => a.UserId == auth.User.Id).ToListAsync());
        Assert.Equal(Jpeg, stored.Data);
    }

    [SkippableFact]
    public async Task Avatar_NotAnImage_ReturnsFieldErrorWhateverTheExtension()
    {
        api.Database.SkipIfUnavailable();
        var (client, _, _) = await LoginAsync(NewEmail(), "Senha123", register: true);

        var response = await UploadAvatarAsync(client, "<svg onload=alert(1)>"u8.ToArray(), "foto.png");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"field\":\"file\"", body);
        Assert.Contains(AvatarService.InvalidTypeMessage, body);
    }

    [SkippableFact]
    public async Task Avatar_WithoutFile_ReturnsFieldError()
    {
        api.Database.SkipIfUnavailable();
        var (client, _, _) = await LoginAsync(NewEmail(), "Senha123", register: true);

        var form = new MultipartFormDataContent { { new StringContent("x"), "outro" } };
        var response = await client.PutAsync("/api/users/me/avatar", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(AvatarService.EmptyFileMessage, await response.Content.ReadAsStringAsync());
    }

    [SkippableFact]
    public async Task Avatar_TooLarge_IsRejectedAndNothingIsSaved()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, _) = await LoginAsync(NewEmail(), "Senha123", register: true);
        var big = new byte[AvatarRules.MaxBytes + 1];
        Jpeg.CopyTo(big, 0);

        var response = await UploadAvatarAsync(client, big, "grande.jpg");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(AvatarService.TooLargeMessage, await response.Content.ReadAsStringAsync());
        await using var db = api.Database.CreateContext();
        Assert.False(await db.UserAvatars.AnyAsync(a => a.UserId == auth.User.Id));
    }

    [SkippableFact]
    public async Task Avatar_Remove_ClearsTheUrlAndTheImage()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, _) = await LoginAsync(NewEmail(), "Senha123", register: true);
        var uploaded = await ReadUserAsync(await UploadAvatarAsync(client, Png));

        var user = await ReadUserAsync(await client.DeleteAsync("/api/users/me/avatar"));

        Assert.Null(user.AvatarUrl);
        Assert.Equal(HttpStatusCode.NotFound, (await api.CreateBrowserClient().GetAsync(uploaded.AvatarUrl)).StatusCode);
        await using var db = api.Database.CreateContext();
        Assert.False(await db.UserAvatars.AnyAsync(a => a.UserId == auth.User.Id));
    }

    [SkippableFact]
    public async Task Avatar_DeletingTheAccount_RemovesThePhoto()
    {
        api.Database.SkipIfUnavailable();
        var (client, auth, _) = await LoginAsync(NewEmail(), "Senha123", register: true);
        await ReadUserAsync(await UploadAvatarAsync(client, Png));

        (await DeleteAccountAsync(client, "Senha123")).EnsureSuccessStatusCode();

        await using var db = api.Database.CreateContext();
        Assert.False(await db.UserAvatars.AnyAsync(a => a.UserId == auth.User.Id));
    }

    [SkippableFact]
    public async Task Avatar_DemoAccountAndAnonymous_AreRefused()
    {
        api.Database.SkipIfUnavailable();
        var (demo, _, _) = await LoginAsync(DemoAccount.Email, DemoAccount.Password, register: false);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await UploadAvatarAsync(demo, Png)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await UploadAvatarAsync(api.CreateBrowserClient(), Png)).StatusCode);
    }
}
