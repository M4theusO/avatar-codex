using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AvatarCodex.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AvatarCodex.IntegrationTests.Api;

/// <summary>API completa sobre um banco descartável com migrations e seed de conteúdo aplicados no startup.</summary>
public sealed class ContentApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TestDatabase Database { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", Database.ConnectionString);
        builder.UseSetting("Database:MigrateAndSeedOnStartup", "true");
        builder.UseSetting("Jwt:Secret", TestSettings.JwtSecret);
    }

    /// <summary>
    /// Cliente com cookies, em https porque o cookie de refresh é Secure
    /// (o TestServer não exige TLS de verdade).
    /// </summary>
    public HttpClient CreateBrowserClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

    public async Task<T> GetJsonAsync<T>(string url)
    {
        var response = await CreateClient().GetAsync(url);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }

    public Task InitializeAsync() => Database.InitializeAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await Database.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class ContentApiCollection : ICollectionFixture<ContentApiFixture>
{
    public const string Name = "Content API";
}
