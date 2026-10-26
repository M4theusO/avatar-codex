using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AvatarCodex.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    // Email e username são citext: as comparações já não diferenciam maiúsculas/minúsculas.
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Username == username, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        db.Users.Include(u => u.Element).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.AsNoTracking().Include(u => u.Element).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken) =>
        db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.Element)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public void Add(User user) => db.Users.Add(user);

    public void AddRefreshToken(RefreshToken token) => db.RefreshTokens.Add(token);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Dois cadastros simultâneos com o mesmo email/username: a unique do banco decide.
            throw new ConflictException("Email ou nome de usuário já cadastrado.");
        }
    }
}
