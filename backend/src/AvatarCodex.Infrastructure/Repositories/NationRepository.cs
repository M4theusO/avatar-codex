using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class NationRepository(AppDbContext db) : INationRepository
{
    public async Task<IReadOnlyList<(Nation Nation, int CharacterCount)>> GetAllAsync(CancellationToken cancellationToken)
    {
        var nations = await db.Nations
            .AsNoTracking()
            .Include(n => n.Element)
            // Ordem dos elementos (Água, Terra, Fogo, Ar); nações sem elemento por último.
            .OrderBy(n => n.ElementId == null)
            .ThenBy(n => n.ElementId)
            .ThenBy(n => n.Name)
            .Select(n => new { Nation = n, CharacterCount = n.Characters.Count })
            .ToListAsync(cancellationToken);

        return [.. nations.Select(n => (n.Nation, n.CharacterCount))];
    }

    public Task<Nation?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
        // Identity resolution preenche Character.Nation de volta para a própria nação, sem Include cíclico.
        db.Nations
            .AsNoTrackingWithIdentityResolution()
            .AsSplitQuery()
            .Include(n => n.Element)
            .Include(n => n.Characters).ThenInclude(c => c.Element)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<string?> GetNameAsync(int id, CancellationToken cancellationToken) =>
        db.Nations.Where(n => n.Id == id).Select(n => n.Name).FirstOrDefaultAsync(cancellationToken);
}
