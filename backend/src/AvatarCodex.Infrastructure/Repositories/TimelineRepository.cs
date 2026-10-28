using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class TimelineRepository(AppDbContext db) : ITimelineRepository
{
    public async Task<IReadOnlyList<(Era Era, int EventCount)>> GetErasAsync(CancellationToken cancellationToken)
    {
        var eras = await db.Eras
            .AsNoTracking()
            .OrderBy(e => e.Order)
            .Select(e => new { Era = e, EventCount = e.Events.Count })
            .ToListAsync(cancellationToken);

        return [.. eras.Select(e => (e.Era, e.EventCount))];
    }

    public async Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(
        int? eraId, int? characterId, CancellationToken cancellationToken)
    {
        var events = db.TimelineEvents.AsNoTracking();
        if (eraId is not null)
            events = events.Where(e => e.EraId == eraId);
        if (characterId is not null)
            events = events.Where(e => e.Characters.Any(c => c.Id == characterId));

        return await events
            .Include(e => e.Era)
            .Include(e => e.Characters).ThenInclude(c => c.Element)
            .OrderBy(e => e.Order)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }
}
