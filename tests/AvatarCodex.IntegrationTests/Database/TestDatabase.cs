using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AvatarCodex.IntegrationTests.Database;

/// <summary>
/// Cria um banco PostgreSQL descartável (com as migrations aplicadas) para cada classe de teste.
/// O servidor vem da variável de ambiente AVATARCODEX_TEST_DB; sem ela, os testes de banco são ignorados.
/// </summary>
public sealed class TestDatabase : IAsyncLifetime
{
    public const string ConnectionStringVariable = "AVATARCODEX_TEST_DB";

    private readonly string? _connectionString;

    public TestDatabase()
    {
        var serverConnection = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(serverConnection))
            return;

        _connectionString = new NpgsqlConnectionStringBuilder(serverConnection)
        {
            Database = $"avatar_codex_test_{Guid.NewGuid():N}"
        }.ConnectionString;
    }

    public bool IsAvailable => _connectionString is not null;

    public void SkipIfUnavailable() =>
        Skip.IfNot(IsAvailable, $"Defina {ConnectionStringVariable} para rodar os testes de banco.");

    public AppDbContext CreateContext(TimeProvider? timeProvider = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;
        return new AppDbContext(options, timeProvider ?? TimeProvider.System);
    }

    public async Task InitializeAsync()
    {
        if (!IsAvailable)
            return;

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (!IsAvailable)
            return;

        await using var db = CreateContext();
        await db.Database.EnsureDeletedAsync();
    }
}
