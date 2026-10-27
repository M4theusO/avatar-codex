using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

/// <summary>Números do usuário que as conquistas medem, lidos numa única avaliação.</summary>
public record ProgressMetrics(
    int ExperiencePoints,
    int CharactersDiscovered,
    int NationsDiscovered,
    int TotalNations,
    int QuizzesCompleted = 0,
    int PerfectQuizzes = 0,
    int CorrectAnswers = 0,
    int ChallengesCompleted = 0);

public interface IAchievementRepository
{
    /// <summary>Catálogo completo, na ordem de exibição.</summary>
    Task<IReadOnlyList<Achievement>> GetCatalogAsync(CancellationToken cancellationToken);

    /// <summary>Conquistas já desbloqueadas pelo usuário: id → data do desbloqueio.</summary>
    Task<IReadOnlyDictionary<int, DateTimeOffset>> GetUnlockedAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Quizzes e desafios ainda não existem: ficam em 0 até as Fases 9 e 11.</summary>
    Task<ProgressMetrics> GetMetricsAsync(Guid userId, CancellationToken cancellationToken);
}
