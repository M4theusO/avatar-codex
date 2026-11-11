using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AvatarCodex.Infrastructure.Repositories;

public class FollowRepository(AppDbContext db) : IFollowRepository
{
    public async Task AddAsync(Guid followerId, Guid followedId, DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        if (await IsFollowingAsync(followerId, followedId, cancellationToken))
            return;

        db.UserFollows.Add(new UserFollow { FollowerId = followerId, FollowedId = followedId, CreatedAt = createdAt });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Seguido em paralelo por outra requisição: o resultado é o mesmo.
            db.ChangeTracker.Clear();
        }
    }

    public Task RemoveAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken) =>
        db.UserFollows
            .Where(f => f.FollowerId == followerId && f.FollowedId == followedId)
            .ExecuteDeleteAsync(cancellationToken);

    public Task<bool> IsFollowingAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken) =>
        db.UserFollows.AnyAsync(f => f.FollowerId == followerId && f.FollowedId == followedId, cancellationToken);

    public async Task<(int Followers, int Following)> GetCountsAsync(Guid userId, CancellationToken cancellationToken) =>
        (await db.UserFollows.CountAsync(f => f.FollowedId == userId, cancellationToken),
         await db.UserFollows.CountAsync(f => f.FollowerId == userId, cancellationToken));

    public async Task<(IReadOnlyList<FollowEntry> Items, int TotalItems)> GetFollowingAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.UserFollows.AsNoTracking().Where(f => f.FollowerId == userId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(f => f.CreatedAt).ThenBy(f => f.Followed!.Username)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => new FollowEntry(
                f.Followed!.Username, f.Followed.AvatarUrl, f.Followed.ExperiencePoints, f.Followed.IsProfilePrivate,
                f.Followed.Element, f.CreatedAt))
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public async Task<(IReadOnlyList<FollowEntry> Items, int TotalItems)> GetFollowersAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = db.UserFollows.AsNoTracking().Where(f => f.FollowedId == userId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(f => f.CreatedAt).ThenBy(f => f.Follower!.Username)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => new FollowEntry(
                f.Follower!.Username, f.Follower.AvatarUrl, f.Follower.ExperiencePoints, f.Follower.IsProfilePrivate,
                f.Follower.Element, f.CreatedAt))
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}
