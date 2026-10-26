using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Data.Seed;

/// <summary>Usuário de demonstração, criado apenas em Development (credenciais no README).</summary>
public class DemoUserSeeder(AppDbContext db, IPasswordHasher passwordHasher)
{
    public const string Email = "demo@avatarcodex.dev";
    public const string Username = "demo";
    public const string Password = "Avatar123";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(u => u.Email == Email, cancellationToken))
            return;

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = Username,
            Email = Email,
            PasswordHash = passwordHasher.Hash(Password)
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
