using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Services;

/// <summary>
/// Regra de cada tipo de conquista (RF-ACH). Um só lugar decide tanto o desbloqueio quanto o progresso
/// exibido nas conquistas bloqueadas, então os dois nunca divergem.
/// </summary>
public static class AchievementRules
{
    public static int Target(Achievement achievement, ProgressMetrics metrics) => achievement.RequirementType switch
    {
        AchievementRequirementType.AccountCreated => 1,
        AchievementRequirementType.AllNationsDiscovered => metrics.TotalNations,
        _ => achievement.RequirementValue
    };

    public static int Current(Achievement achievement, ProgressMetrics metrics) => achievement.RequirementType switch
    {
        AchievementRequirementType.AccountCreated => 1,
        AchievementRequirementType.CharactersDiscovered => metrics.CharactersDiscovered,
        AchievementRequirementType.NationsDiscovered or AchievementRequirementType.AllNationsDiscovered => metrics.NationsDiscovered,
        AchievementRequirementType.QuizzesCompleted => metrics.QuizzesCompleted,
        AchievementRequirementType.PerfectQuizzes => metrics.PerfectQuizzes,
        AchievementRequirementType.CorrectAnswers => metrics.CorrectAnswers,
        AchievementRequirementType.ChallengesCompleted => metrics.ChallengesCompleted,
        AchievementRequirementType.LevelReached => LevelCalculator.GetLevel(metrics.ExperiencePoints),
        _ => throw new ArgumentOutOfRangeException(nameof(achievement), achievement.RequirementType, null)
    };

    /// <summary>Meta zerada (ex.: nenhuma nação cadastrada) nunca desbloqueia.</summary>
    public static bool IsMet(Achievement achievement, ProgressMetrics metrics)
    {
        var target = Target(achievement, metrics);
        return target > 0 && Current(achievement, metrics) >= target;
    }

    /// <summary>Progresso exibido: limitado à meta (12/10 vira 10/10).</summary>
    public static int Progress(Achievement achievement, ProgressMetrics metrics) =>
        Math.Min(Current(achievement, metrics), Target(achievement, metrics));
}
