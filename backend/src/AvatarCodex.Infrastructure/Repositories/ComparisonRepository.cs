using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class ComparisonRepository(AppDbContext db) : IComparisonRepository
{
    public async Task<IReadOnlyList<ComparisonAttribute>> GetAttributesAsync(CancellationToken cancellationToken) =>
        await db.ComparisonAttributes
            .AsNoTracking()
            .OrderBy(a => a.Order)
            .ThenBy(a => a.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Character>> GetCharactersAsync(
        IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
        await db.Characters
            .AsNoTracking()
            .AsSplitQuery()
            .Where(c => ids.Contains(c.Id))
            .Include(c => c.Nation)
            .Include(c => c.Element)
            .Include(c => c.BendingSubtypes)
            .Include(c => c.Abilities).ThenInclude(a => a.Element)
            .Include(c => c.Series)
            .Include(c => c.AttributeValues)
            .ToListAsync(cancellationToken);
}
