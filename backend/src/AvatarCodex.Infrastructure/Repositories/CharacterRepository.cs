using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class CharacterRepository(AppDbContext db) : ICharacterRepository
{
    public async Task<(IReadOnlyList<Character> Items, int TotalItems)> GetPagedAsync(
        CharacterFilter filter, CancellationToken cancellationToken)
    {
        var query = ApplyFilters(db.Characters.AsNoTracking(), filter);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await ApplySorting(query, filter)
            .Include(c => c.Nation)
            .Include(c => c.Element)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public Task<Character?> GetDetailsAsync(int id, CancellationToken cancellationToken) =>
        db.Characters
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Nation)
            .Include(c => c.Element)
            .Include(c => c.BendingSubtypes)
            .Include(c => c.Abilities).ThenInclude(a => a.Element)
            .Include(c => c.Series)
            .Include(c => c.Relationships).ThenInclude(r => r.RelatedCharacter).ThenInclude(rc => rc.Element)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        db.Characters.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<string?> GetNameAsync(int id, CancellationToken cancellationToken) =>
        db.Characters.Where(c => c.Id == id).Select(c => c.Name).FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Ability>> GetAbilitiesAsync(int characterId, CancellationToken cancellationToken) =>
        await db.Characters
            .AsNoTracking()
            .Where(c => c.Id == characterId)
            .SelectMany(c => c.Abilities)
            .Include(a => a.Element)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

    private static IQueryable<Character> ApplyFilters(IQueryable<Character> query, CharacterFilter filter)
    {
        if (filter.Search is not null)
        {
            var pattern = $"%{EscapeLikePattern(filter.Search)}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.Name, pattern, @"\") ||
                c.Aliases.Any(alias => EF.Functions.ILike(alias, pattern, @"\")));
        }

        if (filter.NationId is { } nationId)
            query = query.Where(c => c.NationId == nationId);

        if (filter.ElementId is { } elementId)
            query = query.Where(c => c.ElementId == elementId);

        if (filter.Status is { } status)
            query = query.Where(c => c.Status == status);

        if (filter.SeriesId is { } seriesId)
            query = query.Where(c => c.Series.Any(s => s.Id == seriesId));

        query = filter.Type switch
        {
            CharacterType.Avatar => query.Where(c => c.IsAvatar),
            CharacterType.Bender => query.Where(c => c.IsBender),
            CharacterType.NonBender => query.Where(c => !c.IsBender),
            _ => query
        };

        return query;
    }

    private static IQueryable<Character> ApplySorting(IQueryable<Character> query, CharacterFilter filter)
    {
        // Personagens sem nação/elemento ficam sempre no fim, em qualquer direção.
        var sorted = (filter.Sort, filter.Descending) switch
        {
            (CharacterSortField.Nation, false) => query.OrderBy(c => c.NationId == null).ThenBy(c => c.Nation!.Name).ThenBy(c => c.Name),
            (CharacterSortField.Nation, true) => query.OrderBy(c => c.NationId == null).ThenByDescending(c => c.Nation!.Name).ThenBy(c => c.Name),
            (CharacterSortField.Element, false) => query.OrderBy(c => c.ElementId == null).ThenBy(c => c.Element!.Name).ThenBy(c => c.Name),
            (CharacterSortField.Element, true) => query.OrderBy(c => c.ElementId == null).ThenByDescending(c => c.Element!.Name).ThenBy(c => c.Name),
            (_, true) => query.OrderByDescending(c => c.Name),
            _ => query.OrderBy(c => c.Name)
        };

        // Desempate estável para a paginação não repetir nem pular itens.
        return sorted.ThenBy(c => c.Id);
    }

    private static string EscapeLikePattern(string value) =>
        value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}
