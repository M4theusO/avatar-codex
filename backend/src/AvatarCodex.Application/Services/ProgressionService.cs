using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Services;

/// <summary>
/// Ponto único de ganho de progresso (docs/architecture.md §3.3): XP idempotente, subida de nível e
/// conquistas. Desafios (Fase 11) entram aqui depois.
/// </summary>
public class ProgressionService(
    IProgressionRepository repository,
    IAchievementRepository achievements,
    TimeProvider timeProvider)
{
    /// <summary>
    /// Registra a atividade, concede o XP do tipo e avalia as conquistas. Retorna null quando a recompensa
    /// já tinha sido concedida (chamada repetida ou simultânea): sem XP e sem erro.
    /// </summary>
    public async Task<RewardResult?> GrantAsync(
        Guid userId,
        ExperienceType type,
        int sourceId,
        string description,
        object activityRecord,
        CancellationToken cancellationToken = default)
    {
        var amount = ExperienceRewards.For(type);
        var totalXp = await repository.SaveWithExperienceAsync(
            activityRecord, NewTransaction(userId, type, sourceId, amount, description), cancellationToken);
        if (totalXp is null)
            return null;

        var (unlocked, totalAfterAchievements) = await UnlockAchievementsAsync(userId, cancellationToken);
        return BuildResult(
            previousXp: totalXp.Value - amount,
            totalXp: totalAfterAchievements ?? totalXp.Value,
            xpGained: amount + unlocked.Sum(a => a.ExperienceReward),
            unlocked);
    }

    /// <summary>
    /// Avalia as conquistas fora de um evento de XP (ex.: "Primeiro passo" no cadastro).
    /// Retorna null quando nada foi desbloqueado.
    /// </summary>
    public async Task<RewardResult?> EvaluateAchievementsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (unlocked, totalXp) = await UnlockAchievementsAsync(userId, cancellationToken);
        if (totalXp is null)
            return null;

        var xpGained = unlocked.Sum(a => a.ExperienceReward);
        return BuildResult(totalXp.Value - xpGained, totalXp.Value, xpGained, unlocked);
    }

    /// <summary>
    /// Desbloqueia as conquistas cumpridas e ainda bloqueadas. As métricas são lidas uma vez, antes do XP das
    /// conquistas: esse XP não dispara nova avaliação (RN-08). Cada desbloqueio grava UserAchievement + XP numa
    /// transação; se outra requisição desbloqueou antes, a unicidade do banco faz ele ser ignorado (RN-05).
    /// </summary>
    private async Task<(List<AchievementRewardDto> Unlocked, int? TotalXp)> UnlockAchievementsAsync(
        Guid userId, CancellationToken cancellationToken)
    {
        var catalog = await achievements.GetCatalogAsync(cancellationToken);
        var alreadyUnlocked = await achievements.GetUnlockedAsync(userId, cancellationToken);
        var pending = catalog.Where(a => !alreadyUnlocked.ContainsKey(a.Id)).ToList();
        if (pending.Count == 0)
            return ([], null);

        var metrics = await achievements.GetMetricsAsync(userId, cancellationToken);
        var unlocked = new List<AchievementRewardDto>();
        int? totalXp = null;

        foreach (var achievement in pending.Where(a => AchievementRules.IsMet(a, metrics)))
        {
            var record = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                UnlockedAt = timeProvider.GetUtcNow()
            };
            var transaction = NewTransaction(
                userId, ExperienceType.AchievementUnlocked, achievement.Id, achievement.ExperienceReward,
                $"Conquista: {achievement.Name}");

            var total = await repository.SaveWithExperienceAsync(record, transaction, cancellationToken);
            if (total is null)
                continue;

            unlocked.Add(new AchievementRewardDto(achievement.Id, achievement.Name, achievement.Icon, achievement.ExperienceReward));
            totalXp = total;
        }

        return (unlocked, totalXp);
    }

    private ExperienceTransaction NewTransaction(
        Guid userId, ExperienceType type, int sourceId, int amount, string description) => new()
    {
        UserId = userId,
        Amount = amount,
        Type = type,
        SourceId = sourceId,
        Description = description,
        CreatedAt = timeProvider.GetUtcNow()
    };

    private static RewardResult BuildResult(
        int previousXp, int totalXp, int xpGained, IReadOnlyList<AchievementRewardDto> unlocked)
    {
        var progress = LevelCalculator.GetProgress(totalXp);
        return new RewardResult(
            xpGained,
            totalXp,
            progress.Level,
            progress.Level > LevelCalculator.GetLevel(previousXp),
            progress.CurrentLevelXp,
            progress.NextLevelXp,
            progress.ProgressPercent,
            unlocked,
            []);
    }
}
