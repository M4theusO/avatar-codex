using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class CatalogRepository(AppDbContext db) : ICatalogRepository
{
    public async Task<IReadOnlyList<Element>> GetElementsAsync(CancellationToken cancellationToken) =>
        await db.Elements
            .AsNoTracking()
            .Include(e => e.BendingSubtypes)
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Ability>> GetAbilitiesAsync(int? elementId, CancellationToken cancellationToken)
    {
        var query = db.Abilities.AsNoTracking().Include(a => a.Element).AsQueryable();

        if (elementId is { } id)
            query = query.Where(a => a.ElementId == id);

        return await query.OrderBy(a => a.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Series>> GetSeriesAsync(CancellationToken cancellationToken) =>
        await db.Series.AsNoTracking().OrderBy(s => s.Id).ToListAsync(cancellationToken);
}
