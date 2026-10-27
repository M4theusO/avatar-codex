using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.DTOs;

public record AchievementRewardDto(int Id, string Name, string Icon, int ExperienceReward);

public record ChallengeRewardDto(int Id, string Title, int ExperienceReward);

/// <summary>Envelope de recompensa (docs/api.md §5). O frontend só exibe; nunca calcula.</summary>
public record RewardResult(
    int XpGained,
    int TotalXp,
    int Level,
    bool LeveledUp,
    int CurrentLevelXp,
    int NextLevelXp,
    int LevelProgress,
    IReadOnlyList<AchievementRewardDto> AchievementsUnlocked,
    IReadOnlyList<ChallengeRewardDto> ChallengesCompleted);

/// <summary>Item do histórico de XP (RF-XP-01: cada movimentação é auditável).</summary>
public record ExperienceTransactionDto(
    long Id,
    int Amount,
    ExperienceType Type,
    string Description,
    DateTimeOffset CreatedAt);

/// <summary>Resposta de POST .../discovery. Repetida ou simultânea: IsNew = false e Reward = null.</summary>
public record DiscoveryResultDto(bool IsNew, RewardResult? Reward);

public enum DiscoveryKind
{
    Character,
    Nation
}

public record DiscoveryDto(
    DiscoveryKind Kind,
    int Id,
    string Name,
    ImageDto? Image,
    ElementSummaryDto? Element,
    DateTimeOffset DiscoveredAt);

public record UserStatisticsDto(
    int DiscoveredCharacters,
    int TotalCharacters,
    int DiscoveredNations,
    int TotalNations,
    int Favorites,
    int UnlockedAchievements,
    int TotalAchievements,
    int CompletedQuizzes,
    int TotalQuizzes,
    int QuizAttempts,
    /// <summary>Respostas corretas / respondidas em todas as tentativas (0–100); null sem tentativas.</summary>
    int? AccuracyPercent);

/// <summary>Paginação simples das listas do usuário (?page=1&amp;pageSize=12).</summary>
public class PageQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
