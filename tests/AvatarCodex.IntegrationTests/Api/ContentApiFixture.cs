using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AvatarCodex.IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Api;

/// <summary>
/// API completa sobre um banco descartável com migrations e seed de conteúdo aplicados no startup.
/// Os desafios diários ficam desativados: eles dependem da data (o sorteio muda a cada dia) e somariam XP
/// aos testes das outras áreas. Os testes de desafios usam o <see cref="ChallengesApiFixture"/>.
/// </summary>
public class ContentApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
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

    protected virtual bool DailyChallengesEnabled => false;

    public async Task InitializeAsync()
    {
        await Database.InitializeAsync();
        if (!Database.IsAvailable)
            return;

        // Sobe a API já aqui: o startup aplica migrations e seed antes do primeiro teste.
        _ = Server;
        if (DailyChallengesEnabled)
            return;

        // Sem modelos ativos, nenhum desafio é sorteado.
        await using var db = Database.CreateContext();
        await db.ChallengeTemplates.ExecuteUpdateAsync(s => s.SetProperty(t => t.IsActive, false));
    }

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

/// <summary>Mesma API, em outro banco, com os desafios diários ativos.</summary>
public sealed class ChallengesApiFixture : ContentApiFixture
{
    protected override bool DailyChallengesEnabled => true;
}

[CollectionDefinition(Name)]
public class ChallengesApiCollection : ICollectionFixture<ChallengesApiFixture>
{
    public const string Name = "Challenges API";
}
