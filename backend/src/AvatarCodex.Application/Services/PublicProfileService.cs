using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;

namespace AvatarCodex.Application.Services;

/// <summary>Quem pode ver o perfil público de quem.</summary>
public static class ProfileRules
{
    /// <summary>Perfil público: qualquer usuário logado. Privado: só o dono.</summary>
    public static bool CanView(User owner, Guid viewerId) => !owner.IsProfilePrivate || owner.Id == viewerId;
}

/// <summary>Perfil público de outro usuário (RF-SOCIAL-01), só para quem está logado.</summary>
public class PublicProfileService(
    IUserRepository users,
    IUserCollectionRepository collection,
    IAchievementRepository achievements)
{
    public const string NotFoundMessage = "Perfil não encontrado.";

    /// <summary>
    /// Perfil inexistente e perfil privado dão o mesmo 404, para não revelar se a conta existe.
    /// </summary>
    public async Task<PublicProfileDto> GetAsync(Guid viewerId, string username, CancellationToken cancellationToken = default)
    {
        var owner = await users.GetByUsernameAsync(username.Trim(), cancellationToken);
        if (owner is null || !ProfileRules.CanView(owner, viewerId))
            throw new NotFoundException(NotFoundMessage);

        var statistics = await collection.GetStatisticsAsync(owner.Id, cancellationToken);
        var catalog = await achievements.GetCatalogAsync(cancellationToken);
        var unlocked = await achievements.GetUnlockedAsync(owner.Id, cancellationToken);
        var progress = LevelCalculator.GetProgress(owner.ExperiencePoints);

        return new PublicProfileDto(
            owner.Username,
            owner.AvatarUrl,
            owner.Element.ToSummaryDto(),
            progress.Level,
            owner.ExperiencePoints,
            progress.CurrentLevelXp,
            progress.NextLevelXp,
            progress.ProgressPercent,
            owner.CreatedAt,
            owner.Id == viewerId,
            owner.IsProfilePrivate,
            statistics,
            // Mais recentes primeiro; empate (mesmo evento) na ordem do catálogo.
            [.. catalog
                .Where(a => unlocked.ContainsKey(a.Id))
                .Select((a, order) => (Achievement: a, Order: order))
                .OrderByDescending(x => unlocked[x.Achievement.Id])
                .ThenBy(x => x.Order)
                .Select(x => new ProfileAchievementDto(
                    x.Achievement.Id, x.Achievement.Name, x.Achievement.Description, x.Achievement.Icon, x.Achievement.ExperienceReward,
                    unlocked[x.Achievement.Id]))]);
    }
}
