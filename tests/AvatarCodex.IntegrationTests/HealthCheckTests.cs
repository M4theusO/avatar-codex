using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AvatarCodex.IntegrationTests;

public class HealthCheckTests(HealthCheckTests.UnreachableDatabaseFactory factory)
    : IClassFixture<HealthCheckTests.UnreachableDatabaseFactory>
{
    [Fact]
    public async Task Health_WhenDatabaseIsUnreachable_ReturnsUnhealthyJson()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("status").GetString());
        Assert.Equal("Unhealthy", body.RootElement.GetProperty("checks").GetProperty("database").GetString());
    }

    public class UnreachableDatabaseFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Default",
                "Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=1");
            builder.UseSetting("Database:MigrateAndSeedOnStartup", "false");
            builder.UseSetting("Jwt:Secret", TestSettings.JwtSecret);
        }
    }
}
