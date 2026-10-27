using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class AchievementRepository(AppDbContext db) : IAchievementRepository
{
    public async Task<IReadOnlyList<Achievement>> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var catalog = await db.Achievements.AsNoTracking().ToListAsync(cancellationToken);

        // Ordena em memória: o tipo é gravado como texto, e a ordem desejada é a do enum
        // (cadastro → descobertas → quizzes → desafios → nível), da meta menor para a maior.
        return [.. catalog
            .OrderBy(a => a.RequirementType)
            .ThenBy(a => a.RequirementValue)
            .ThenBy(a => a.Id)];
    }

    public async Task<IReadOnlyDictionary<int, DateTimeOffset>> GetUnlockedAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.UserAchievements
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .ToDictionaryAsync(u => u.AchievementId, u => u.UnlockedAt, cancellationToken);

    public async Task<ProgressMetrics> GetMetricsAsync(Guid userId, CancellationToken cancellationToken) =>
        new(
            ExperiencePoints: await db.Users.Where(u => u.Id == userId).Select(u => u.ExperiencePoints).SingleAsync(cancellationToken),
            CharactersDiscovered: await db.CharacterDiscoveries.CountAsync(d => d.UserId == userId, cancellationToken),
            NationsDiscovered: await db.NationDiscoveries.CountAsync(d => d.UserId == userId, cancellationToken),
            TotalNations: await db.Nations.CountAsync(cancellationToken),
            QuizzesCompleted: await db.QuizAttempts.Where(a => a.UserId == userId).Select(a => a.QuizId).Distinct().CountAsync(cancellationToken),
            PerfectQuizzes: await db.QuizAttempts
                .Where(a => a.UserId == userId && a.Score == a.TotalQuestions)
                .Select(a => a.QuizId).Distinct().CountAsync(cancellationToken),
            // Todas as tentativas contam, inclusive quizzes refeitos (ver docs/development-phases.md, Fase 9).
            CorrectAnswers: await db.QuizAnswers.CountAsync(
                a => a.IsCorrect && db.QuizAttempts.Any(t => t.Id == a.UserQuizAttemptId && t.UserId == userId), cancellationToken),
            ChallengesCompleted: await db.UserDailyChallenges.CountAsync(u => u.UserId == userId, cancellationToken));
}
