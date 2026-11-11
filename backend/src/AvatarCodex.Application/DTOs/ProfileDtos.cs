namespace AvatarCodex.Application.DTOs;

/// <summary>Conquista desbloqueada, como aparece no perfil público.</summary>
public record ProfileAchievementDto(int Id, string Name, string Description, string Icon, int ExperienceReward, DateTimeOffset UnlockedAt);

/// <summary>
/// Perfil público (GET /api/users/{username}): só dados do jogo. Nunca email, id ou histórico detalhado.
/// IsCurrentUser = quem consulta é o dono do perfil (o frontend oferece "Editar perfil");
/// IsFollowing = quem consulta segue este perfil (sempre false no próprio).
/// </summary>
public record PublicProfileDto(
    string Username,
    string? AvatarUrl,
    ElementSummaryDto? Element,
    int Level,
    int ExperiencePoints,
    int CurrentLevelXp,
    int NextLevelXp,
    int LevelProgress,
    DateTimeOffset CreatedAt,
    bool IsCurrentUser,
    bool IsProfilePrivate,
    int FollowersCount,
    int FollowingCount,
    bool IsFollowing,
    UserStatisticsDto Statistics,
    IReadOnlyList<ProfileAchievementDto> Achievements);
