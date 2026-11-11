using AvatarCodex.Application.Interfaces;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class RankingRepository(AppDbContext db) : IRankingRepository
{
    /// <summary>Projeção com setters (e não construtor) para o EF conseguir filtrar e ordenar depois dela.</summary>
    private sealed class Row
    {
        public Guid UserId { get; init; }
        public string Username { get; init; } = "";
        public string? AvatarUrl { get; init; }
        public int TotalExperience { get; init; }
        public int Score { get; init; }
        public int? ElementId { get; init; }
        public string? ElementName { get; init; }
        public string? ElementColor { get; init; }
        public string? ElementIcon { get; init; }
        public bool IsProfilePrivate { get; init; }
    }

    /// <summary>Quem entra no ranking: XP no período maior que zero (e do elemento, se filtrado).</summary>
    private IQueryable<Row> Query(RankingFilter filter)
    {
        var users = db.Users.AsNoTracking();
        if (filter.ElementId is { } elementId)
            users = users.Where(u => u.ElementId == elementId);
        if (filter.FollowedBy is { } viewerId)
            users = users.Where(u => u.Id == viewerId
                || db.UserFollows.Any(f => f.FollowerId == viewerId && f.FollowedId == u.Id));

        IQueryable<Row> rows;
        if (filter.From is { } from && filter.To is { } to)
        {
            var monthly = db.ExperienceTransactions
                .Where(t => t.CreatedAt >= from && t.CreatedAt < to)
                .GroupBy(t => t.UserId)
                .Select(g => new { UserId = g.Key, Score = g.Sum(t => t.Amount) });

            rows = from user in users
                   join month in monthly on user.Id equals month.UserId
                   select new Row
                   {
                       UserId = user.Id,
                       Username = user.Username,
                       AvatarUrl = user.AvatarUrl,
                       TotalExperience = user.ExperiencePoints,
                       Score = month.Score,
                       ElementId = user.ElementId,
                       ElementName = user.Element != null ? user.Element.Name : null,
                       ElementColor = user.Element != null ? user.Element.Color : null,
                       ElementIcon = user.Element != null ? user.Element.Icon : null,
                       IsProfilePrivate = user.IsProfilePrivate
                   };
        }
        else
        {
            rows = users.Select(user => new Row
            {
                UserId = user.Id,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                TotalExperience = user.ExperiencePoints,
                Score = user.ExperiencePoints,
                ElementId = user.ElementId,
                ElementName = user.Element != null ? user.Element.Name : null,
                ElementColor = user.Element != null ? user.Element.Color : null,
                ElementIcon = user.Element != null ? user.Element.Icon : null,
                IsProfilePrivate = user.IsProfilePrivate
            });
        }

        return rows.Where(r => r.Score > 0);
    }

    public async Task<(IReadOnlyList<RankingRow> Rows, int TotalItems)> GetPageAsync(
        RankingFilter filter, int skip, int take, CancellationToken cancellationToken)
    {
        var query = Query(filter);
        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Username)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return ([.. rows.Select(r => new RankingRow(
            r.UserId, r.Username, r.AvatarUrl, r.TotalExperience, r.Score, r.ElementId, r.ElementName, r.ElementColor, r.ElementIcon, r.IsProfilePrivate))], total);
    }

    public Task<int> CountAboveAsync(RankingFilter filter, int score, CancellationToken cancellationToken) =>
        Query(filter).CountAsync(r => r.Score > score, cancellationToken);

    public Task<int?> GetScoreAsync(RankingFilter filter, Guid userId, CancellationToken cancellationToken) =>
        Query(filter).Where(r => r.UserId == userId).Select(r => (int?)r.Score).FirstOrDefaultAsync(cancellationToken);
}
