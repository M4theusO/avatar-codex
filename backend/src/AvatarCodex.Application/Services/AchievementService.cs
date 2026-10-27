using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;

namespace AvatarCodex.Application.Services;

/// <summary>Leitura do catálogo e das conquistas do usuário. O desbloqueio fica no ProgressionService.</summary>
public class AchievementService(IAchievementRepository achievements)
{
    public async Task<IReadOnlyList<AchievementDto>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await achievements.GetCatalogAsync(cancellationToken);
        return [.. catalog.Select(a => new AchievementDto(a.Id, a.Name, a.Description, a.Icon, a.ExperienceReward))];
    }

    public async Task<IReadOnlyList<UserAchievementDto>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var catalog = await achievements.GetCatalogAsync(cancellationToken);
        var unlocked = await achievements.GetUnlockedAsync(userId, cancellationToken);
        var metrics = await achievements.GetMetricsAsync(userId, cancellationToken);

        return [.. catalog.Select(a =>
        {
            var target = AchievementRules.Target(a, metrics);
            var isUnlocked = unlocked.TryGetValue(a.Id, out var unlockedAt);
            return new UserAchievementDto(
                a.Id,
                a.Name,
                a.Description,
                a.Icon,
                a.ExperienceReward,
                isUnlocked,
                isUnlocked ? unlockedAt : null,
                // Desbloqueada fica cheia mesmo se a meta mudar depois (ex.: nova nação no conteúdo).
                isUnlocked ? target : AchievementRules.Progress(a, metrics),
                target);
        })];
    }
}
