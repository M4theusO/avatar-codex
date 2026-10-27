using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AvatarCodex.Infrastructure.Repositories;

public class ChallengeRepository(AppDbContext db) : IChallengeRepository
{
    public async Task<IReadOnlyList<ChallengeTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken) =>
        await db.ChallengeTemplates
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DailyChallenge>> GetForDateAsync(DateOnly date, CancellationToken cancellationToken) =>
        await db.DailyChallenges
            .AsNoTracking()
            .Include(d => d.Template)
            .Where(d => d.Date == date)
            .OrderBy(d => d.Id)
            .ToListAsync(cancellationToken);

    public async Task AddForDateAsync(DateOnly date, IReadOnlyList<int> templateIds, CancellationToken cancellationToken)
    {
        db.DailyChallenges.AddRange(templateIds.Select(id => new DailyChallenge { ChallengeTemplateId = id, Date = date }));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Gerados ao mesmo tempo por outra requisição: o sorteio só depende da data, então são os mesmos.
            db.ChangeTracker.Clear();
        }
    }

    public async Task<IReadOnlyDictionary<int, DateTimeOffset>> GetCompletedAsync(
        Guid userId, IReadOnlyCollection<int> dailyChallengeIds, CancellationToken cancellationToken) =>
        await db.UserDailyChallenges
            .AsNoTracking()
            .Where(u => u.UserId == userId && dailyChallengeIds.Contains(u.DailyChallengeId))
            .ToDictionaryAsync(u => u.DailyChallengeId, u => u.CompletedAt, cancellationToken);

    public async Task<DailyActivity> GetActivityAsync(
        Guid userId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        var attempts = await db.QuizAttempts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.CompletedAt >= from && a.CompletedAt < to)
            .Select(a => new { a.QuizId, a.Score, a.TotalQuestions })
            .ToListAsync(cancellationToken);

        return new DailyActivity(
            CharactersDiscovered: await db.CharacterDiscoveries.CountAsync(
                d => d.UserId == userId && d.DiscoveredAt >= from && d.DiscoveredAt < to, cancellationToken),
            NationsDiscovered: await db.NationDiscoveries.CountAsync(
                d => d.UserId == userId && d.DiscoveredAt >= from && d.DiscoveredAt < to, cancellationToken),
            QuizzesCompleted: attempts.Select(a => a.QuizId).Distinct().Count(),
            BestQuizPercentage: attempts.Count == 0 ? 0 : attempts.Max(a => QuizScoring.Percentage(a.Score, a.TotalQuestions)));
    }

    public async Task<(IReadOnlyList<UserDailyChallenge> Items, int TotalItems)> GetHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var completed = db.UserDailyChallenges.AsNoTracking().Where(u => u.UserId == userId);

        var totalItems = await completed.CountAsync(cancellationToken);
        var items = await completed
            .Include(u => u.DailyChallenge).ThenInclude(d => d.Template)
            .OrderByDescending(u => u.CompletedAt)
            .ThenByDescending(u => u.DailyChallengeId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }
}
