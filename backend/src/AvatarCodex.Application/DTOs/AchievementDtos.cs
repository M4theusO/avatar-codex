namespace AvatarCodex.Application.DTOs;

/// <summary>Item do catálogo público de conquistas.</summary>
public record AchievementDto(int Id, string Name, string Description, string Icon, int ExperienceReward);

/// <summary>
/// Conquista do ponto de vista do usuário: desbloqueada (com data) ou bloqueada (com progresso).
/// Progress/Target vêm prontos do backend; o frontend só desenha a barra.
/// </summary>
public record UserAchievementDto(
    int Id,
    string Name,
    string Description,
    string Icon,
    int ExperienceReward,
    bool IsUnlocked,
    DateTimeOffset? UnlockedAt,
    int Progress,
    int Target);
