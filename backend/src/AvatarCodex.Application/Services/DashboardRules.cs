using AvatarCodex.Application.DTOs;

namespace AvatarCodex.Application.Services;

/// <summary>Escolhas do dashboard: o que recomendar e o que destacar. Puras, para testar sem banco.</summary>
public static class DashboardRules
{
    /// <summary>
    /// Primeiro quiz ainda não concluído (na ordem do catálogo). Se todos já foram concluídos, o de melhor
    /// resultado que ainda não foi gabaritado; se todos foram gabaritados, null.
    /// </summary>
    public static QuizListItemDto? FeaturedQuiz(IReadOnlyList<QuizListItemDto> quizzes) =>
        quizzes.FirstOrDefault(q => q.BestResult is null)
        ?? quizzes
            .Where(q => q.BestResult!.Percentage < 100)
            .OrderByDescending(q => q.BestResult!.Percentage)
            .FirstOrDefault();

    /// <summary>A conquista bloqueada com a maior fração da meta já cumprida (empate: ordem do catálogo).</summary>
    public static UserAchievementDto? NextAchievement(IReadOnlyList<UserAchievementDto> achievements) =>
        achievements
            .Where(a => !a.IsUnlocked)
            .OrderByDescending(a => a.Target == 0 ? 0 : (double)a.Progress / a.Target)
            .FirstOrDefault();

    /// <summary>
    /// Sorteio estável por usuário e por dia: a lista não muda a cada recarga, mas muda no dia seguinte.
    /// Os candidatos devem vir numa ordem fixa (ex.: por id).
    /// </summary>
    public static IReadOnlyList<T> PickDaily<T>(IReadOnlyList<T> candidates, Guid userId, DateOnly day, int count)
    {
        // Guid.GetHashCode depende só dos bytes do Guid (não é aleatório por processo, como HashCode.Combine).
        var random = new Random(userId.GetHashCode() ^ day.DayNumber);
        return [.. candidates.Select(c => (Item: c, Key: random.Next())).OrderBy(c => c.Key).Take(count).Select(c => c.Item)];
    }
}
