using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AvatarCodex.Infrastructure.Repositories;

public class ProgressionRepository(AppDbContext db) : IProgressionRepository
{
    public async Task<int?> SaveWithExperienceAsync(
        object? activityRecord, ExperienceTransaction transaction, CancellationToken cancellationToken)
    {
        await using var dbTransaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (activityRecord is not null)
                db.Add(activityRecord);
            db.ExperienceTransactions.Add(transaction);
            await db.SaveChangesAsync(cancellationToken);

            // Incremento no próprio UPDATE: duas recompensas simultâneas diferentes não perdem XP.
            await db.Users
                .Where(u => u.Id == transaction.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ExperiencePoints, u => u.ExperiencePoints + transaction.Amount),
                    cancellationToken);

            var totalXp = await db.Users
                .Where(u => u.Id == transaction.UserId)
                .Select(u => u.ExperiencePoints)
                .SingleAsync(cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);
            return totalXp;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Outra requisição já registrou a mesma atividade/recompensa: "já concedido" (RN-12 d).
            await dbTransaction.RollbackAsync(cancellationToken);
            db.ChangeTracker.Clear();
            return null;
        }
    }
}
