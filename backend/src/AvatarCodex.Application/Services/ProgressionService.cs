using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Services;

/// <summary>
/// Uma recompensa de XP de um evento. ActivityRecord (ex.: a descoberta) é gravado junto com o XP,
/// na mesma transação; null quando a atividade já foi gravada antes (ex.: a tentativa de quiz).
/// </summary>
public record ExperienceGrant(ExperienceType Type, int SourceId, string Description, object? ActivityRecord = null);

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
    /// já tinha sido concedida (chamada repetida ou simultânea): sem XP, sem erro e sem nova avaliação.
    /// </summary>
    public async Task<RewardResult?> GrantAsync(
        Guid userId,
        ExperienceType type,
        int sourceId,
        string description,
        object activityRecord,
        CancellationToken cancellationToken = default)
    {
        var granted = await GrantExperienceAsync(userId, [new ExperienceGrant(type, sourceId, description, activityRecord)], cancellationToken);
        return granted.TotalXp is null
            ? null
            : await FinishAsync(userId, granted.XpGained, granted.TotalXp, cancellationToken);
    }

    /// <summary>
    /// Evento com várias recompensas possíveis (ex.: quiz concluído + perfeito). As já concedidas são
    /// ignoradas, e as conquistas são avaliadas mesmo sem XP novo: o evento pode ter mudado as métricas
    /// (ex.: respostas corretas de um quiz refeito). Retorna null quando nada foi ganho.
    /// </summary>
    public async Task<RewardResult?> GrantAllAsync(
        Guid userId, IReadOnlyList<ExperienceGrant> grants, CancellationToken cancellationToken = default)
    {
        var granted = await GrantExperienceAsync(userId, grants, cancellationToken);
        return await FinishAsync(userId, granted.XpGained, granted.TotalXp, cancellationToken);
    }

    /// <summary>
    /// Avalia as conquistas fora de um evento de XP (ex.: "Primeiro passo" no cadastro).
    /// Retorna null quando nada foi desbloqueado.
    /// </summary>
    public Task<RewardResult?> EvaluateAchievementsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        FinishAsync(userId, 0, null, cancellationToken);

    private async Task<(int XpGained, int? TotalXp)> GrantExperienceAsync(
        Guid userId, IReadOnlyList<ExperienceGrant> grants, CancellationToken cancellationToken)
    {
        var xpGained = 0;
        int? totalXp = null;

        foreach (var grant in grants)
        {
            var amount = ExperienceRewards.For(grant.Type);
            var total = await repository.SaveWithExperienceAsync(
                grant.ActivityRecord, NewTransaction(userId, grant.Type, grant.SourceId, amount, grant.Description), cancellationToken);
            if (total is null)
                continue;

            xpGained += amount;
            totalXp = total;
        }

        return (xpGained, totalXp);
    }

    /// <summary>Desbloqueia as conquistas e monta o reward; null quando não houve XP nem conquista.</summary>
    private async Task<RewardResult?> FinishAsync(Guid userId, int xpGained, int? totalXp, CancellationToken cancellationToken)
    {
        var (unlocked, totalAfterAchievements) = await UnlockAchievementsAsync(userId, cancellationToken);
        var finalXp = totalAfterAchievements ?? totalXp;
        if (finalXp is null)
            return null;

        var totalGained = xpGained + unlocked.Sum(a => a.ExperienceReward);
        var progress = LevelCalculator.GetProgress(finalXp.Value);
        return new RewardResult(
            totalGained,
            finalXp.Value,
            progress.Level,
            progress.Level > LevelCalculator.GetLevel(finalXp.Value - totalGained),
            progress.CurrentLevelXp,
            progress.NextLevelXp,
            progress.ProgressPercent,
            unlocked,
            []);
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
}
