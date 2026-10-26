using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

/// <summary>Usuários e suas sessões (refresh tokens).</summary>
public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken);

    /// <summary>Usuário com Element carregado, ou null. A comparação não diferencia maiúsculas/minúsculas.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Usuário com Element carregado, ou null.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Refresh token com o usuário (e o Element dele) carregado, ou null.</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken);

    void Add(User user);

    void AddRefreshToken(RefreshToken token);

    /// <summary>Salva as alterações. Violação de email/username únicos vira ConflictException.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}

public record AccessToken(string Token, int ExpiresInSeconds);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);

    /// <summary>Valor aleatório e opaco, entregue ao cliente apenas pelo cookie.</summary>
    string CreateRefreshToken();

    /// <summary>Hash guardado no banco no lugar do refresh token.</summary>
    string HashRefreshToken(string refreshToken);

    TimeSpan RefreshTokenLifetime { get; }
}
