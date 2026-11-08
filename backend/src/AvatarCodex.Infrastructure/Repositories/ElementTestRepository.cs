using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class ElementTestRepository(AppDbContext db) : IElementTestRepository
{
    public async Task<IReadOnlyList<ElementTestQuestion>> GetQuestionsAsync(CancellationToken cancellationToken) =>
        await db.ElementTestQuestions
            .AsNoTracking()
            .AsSplitQuery()
            .Include(q => q.Options.OrderBy(o => o.Order))
            .OrderBy(q => q.Order)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Element>> GetElementsAsync(CancellationToken cancellationToken) =>
        await db.Elements
            .AsNoTracking()
            .Include(e => e.BendingSubtypes)
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);

    public async Task SetUserElementAsync(Guid userId, int elementId, CancellationToken cancellationToken)
    {
        // Pela entidade rastreada (e não ExecuteUpdate) para o UpdatedAt ser preenchido pela auditoria.
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedException("Sua sessão expirou. Entre novamente.");
        user.ElementId = elementId;
        await db.SaveChangesAsync(cancellationToken);
    }
}
