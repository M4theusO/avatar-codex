using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Domain.Entities;

/// <summary>
/// Movimentação de XP auditável (RF-XP-01). A unique (UserId, Type, SourceId) garante que
/// a mesma recompensa nunca é concedida duas vezes (RN-05).
/// </summary>
public class ExperienceTransaction
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public ExperienceType Type { get; set; }

    /// <summary>Id do personagem/nação/quiz/desafio/conquista que gerou o XP.</summary>
    public int SourceId { get; set; }

    public required string Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
