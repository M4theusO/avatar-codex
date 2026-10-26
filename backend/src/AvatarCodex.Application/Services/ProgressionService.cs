using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Services;

/// <summary>
/// Ponto único de ganho de progresso (docs/architecture.md §3.3). Nesta fase: XP idempotente e
/// subida de nível. Desafios (Fase 11) e conquistas (Fase 8) entram aqui depois.
/// </summary>
public class ProgressionService(IProgressionRepository repository, TimeProvider timeProvider)
{
    /// <summary>
    /// Registra a atividade e concede o XP do tipo. Retorna null quando a recompensa já tinha sido
    /// concedida (chamada repetida ou simultânea): sem XP e sem erro.
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
        var transaction = new ExperienceTransaction
        {
            UserId = userId,
            Amount = amount,
            Type = type,
            SourceId = sourceId,
            Description = description,
            CreatedAt = timeProvider.GetUtcNow()
        };

        var totalXp = await repository.SaveWithExperienceAsync(activityRecord, transaction, cancellationToken);
        if (totalXp is null)
            return null;

        var previousLevel = LevelCalculator.GetLevel(totalXp.Value - amount);
        var level = LevelCalculator.GetLevel(totalXp.Value);
        return new RewardResult(amount, totalXp.Value, level, level > previousLevel, [], []);
    }
}
