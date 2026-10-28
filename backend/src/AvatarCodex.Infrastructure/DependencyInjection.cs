using AvatarCodex.Application.Interfaces;
using AvatarCodex.Infrastructure.Data;
using AvatarCodex.Infrastructure.Repositories;
using AvatarCodex.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AvatarCodex.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' não configurada. Veja a seção 'Variáveis de ambiente' do README.");

        services.AddSingleton(TimeProvider.System);

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<INationRepository, NationRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProgressionRepository, ProgressionRepository>();
        services.AddScoped<IUserCollectionRepository, UserCollectionRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IChallengeRepository, ChallengeRepository>();
        services.AddScoped<ITimelineRepository, TimelineRepository>();

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        return services;
    }
}
