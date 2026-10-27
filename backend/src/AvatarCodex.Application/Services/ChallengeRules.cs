using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Services;

/// <summary>
/// Regras dos desafios diários (RF-CHAL, RN-10): sorteio do dia, progresso e conclusão. O mesmo código
/// calcula o progresso exibido e decide a conclusão, então os dois nunca divergem.
/// </summary>
public static class ChallengeRules
{
    public const int ChallengesPerDay = 3;

    public static int Current(ChallengeTemplate template, DailyActivity activity) => template.ChallengeType switch
    {
        ChallengeType.DiscoverCharacters => activity.CharactersDiscovered,
        ChallengeType.DiscoverNations => activity.NationsDiscovered,
        ChallengeType.CompleteQuizzes => activity.QuizzesCompleted,
        ChallengeType.QuizScoreAtLeast => activity.BestQuizPercentage,
        _ => throw new ArgumentOutOfRangeException(nameof(template), template.ChallengeType, null)
    };

    public static bool IsMet(ChallengeTemplate template, DailyActivity activity) =>
        template.TargetValue > 0 && Current(template, activity) >= template.TargetValue;

    /// <summary>Progresso exibido: limitado à meta (5 de 3 vira 3 de 3).</summary>
    public static int Progress(ChallengeTemplate template, DailyActivity activity) =>
        Math.Min(Current(template, activity), template.TargetValue);

    /// <summary>
    /// Sorteio do dia: depende só da data, então duas gerações simultâneas escolhem os mesmos modelos.
    /// Prefere tipos diferentes (variedade); se houver menos tipos que desafios, completa com os restantes.
    /// Os modelos devem vir numa ordem fixa (ex.: por id).
    /// </summary>
    public static IReadOnlyList<ChallengeTemplate> PickForDay(
        IReadOnlyList<ChallengeTemplate> templates, DateOnly day, int count = ChallengesPerDay)
    {
        var random = new Random(day.DayNumber);
        var shuffled = templates
            .Select(t => (Template: t, Key: random.Next()))
            .OrderBy(t => t.Key)
            .Select(t => t.Template)
            .ToList();

        var picked = shuffled.DistinctBy(t => t.ChallengeType).Take(count).ToList();
        picked.AddRange(shuffled.Except(picked).Take(count - picked.Count));
        return picked;
    }

    /// <summary>Início (inclusive) e fim (exclusivo) do dia em UTC.</summary>
    public static (DateTimeOffset From, DateTimeOffset To) DayRange(DateOnly day)
    {
        var from = new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        return (from, from.AddDays(1));
    }

    public static DateOnly Today(TimeProvider timeProvider) => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
