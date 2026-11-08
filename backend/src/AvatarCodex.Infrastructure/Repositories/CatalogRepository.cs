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

    public async Task<IReadOnlyList<(Ability Ability, int CharacterCount)>> GetAbilitiesAsync(
        int? elementId, CancellationToken cancellationToken)
    {
        var query = db.Abilities.AsNoTracking().Include(a => a.Element).AsQueryable();

        if (elementId is { } id)
            query = query.Where(a => a.ElementId == id);

        var abilities = await query
            // Mesma ordem das nações: Água, Terra, Fogo, Ar; sem elemento por último.
            .OrderBy(a => a.ElementId == null)
            .ThenBy(a => a.ElementId)
            .ThenBy(a => a.Name)
            .Select(a => new { Ability = a, CharacterCount = a.Characters.Count })
            .ToListAsync(cancellationToken);

        return [.. abilities.Select(a => (a.Ability, a.CharacterCount))];
    }

    public Task<Ability?> GetAbilityDetailsAsync(int id, CancellationToken cancellationToken) =>
        db.Abilities
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Element)
            .Include(a => a.Characters).ThenInclude(c => c.Nation)
            .Include(a => a.Characters).ThenInclude(c => c.Element)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Series>> GetSeriesAsync(CancellationToken cancellationToken) =>
        await db.Series.AsNoTracking().OrderBy(s => s.Id).ToListAsync(cancellationToken);
}
