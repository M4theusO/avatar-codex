using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Data.Seed;

/// <summary>Usuário de demonstração (credenciais no README). Desligável com Database:SeedDemoUser = false.</summary>
public class DemoUserSeeder(AppDbContext db, IPasswordHasher passwordHasher)
{
    public const string Email = DemoAccount.Email;
    public const string Username = DemoAccount.Username;
    public const string Password = DemoAccount.Password;

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
