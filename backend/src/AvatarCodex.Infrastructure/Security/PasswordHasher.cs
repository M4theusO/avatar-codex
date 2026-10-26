using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AvatarCodex.Infrastructure.Security;

/// <summary>PBKDF2 do ASP.NET Core Identity (sal aleatório e iterações embutidos no hash), sem o Identity completo.</summary>
public class PasswordHasher : IPasswordHasher
{
    private static readonly PasswordHasher<User> Hasher = new();

    public string Hash(string password) => Hasher.HashPassword(null!, password);

    public bool Verify(string passwordHash, string password) =>
        Hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
