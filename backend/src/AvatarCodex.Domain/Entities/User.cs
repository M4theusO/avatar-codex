using AvatarCodex.Domain.Interfaces;

namespace AvatarCodex.Domain.Entities;

public class User : IAuditableEntity
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }

    public int? ElementId { get; set; }
    public Element? Element { get; set; }

    /// <summary>Total desnormalizado das ExperienceTransactions. O nível é calculado a partir dele.</summary>
    public int ExperiencePoints { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
