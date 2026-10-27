using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Domain.Entities;

/// <summary>Conquista do catálogo (conteúdo do seed). Code é a chave estável usada pelo seed.</summary>
public class Achievement
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    /// <summary>Nome de um ícone do lucide-react (ex.: "compass").</summary>
    public required string Icon { get; set; }

    public AchievementRequirementType RequirementType { get; set; }
    public int RequirementValue { get; set; }
    public int ExperienceReward { get; set; }
}

/// <summary>Desbloqueio de uma conquista. A PK (UserId, AchievementId) impede desbloquear duas vezes (RN-05).</summary>
public class UserAchievement
{
    public Guid UserId { get; set; }
    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = null!;
    public DateTimeOffset UnlockedAt { get; set; }
}
