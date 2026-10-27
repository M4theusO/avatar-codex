using AvatarCodex.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AvatarCodex.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<CharacterService>();
        services.AddScoped<NationService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<AuthService>();
        services.AddScoped<ProgressionService>();
        services.AddScoped<DiscoveryService>();
        services.AddScoped<FavoriteService>();
        services.AddScoped<UserService>();
        services.AddScoped<AchievementService>();

        return services;
    }
}
