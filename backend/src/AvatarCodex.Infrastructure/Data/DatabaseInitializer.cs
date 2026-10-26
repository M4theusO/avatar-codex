using AvatarCodex.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AvatarCodex.Infrastructure.Data;

public static class DatabaseInitializer
{
    /// <summary>Aplica as migrations pendentes e o seed de conteúdo. Usado apenas em Development.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);
        await new ContentSeeder(db).SeedAsync(ContentSeedData.Load(), cancellationToken);
    }
}
