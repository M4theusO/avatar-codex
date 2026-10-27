using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.Application.Services;

public class AuthService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokens,
    ProgressionService progression,
    TimeProvider timeProvider,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator)
{
    // Mesma mensagem para email inexistente e senha errada: não revela quais emails têm conta.
    public const string InvalidCredentialsMessage = "Email ou senha inválidos.";
    public const string InvalidSessionMessage = "Sua sessão expirou. Entre novamente.";

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        request = request with
        {
            Username = request.Username?.Trim() ?? "",
            Email = NormalizeEmail(request.Email)
        };
        await registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        // RN-01 (a unique do banco cobre a corrida entre dois cadastros simultâneos)
        if (await users.EmailExistsAsync(request.Email, cancellationToken))
            throw new ConflictException("Este email já está cadastrado.");
        if (await users.UsernameExistsAsync(request.Username, cancellationToken))
            throw new ConflictException("Este nome de usuário já está em uso.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password)
        };
        users.Add(user);
        await users.SaveChangesAsync(cancellationToken);

        // RF-ACH-01: "Primeiro passo" é desbloqueado no cadastro, pelo mesmo serviço de progressão.
        var reward = await progression.EvaluateAchievementsAsync(user.Id, cancellationToken);
        if (reward is not null)
            user.ExperiencePoints = reward.TotalXp; // o banco já tem esse total; só alinha a entidade em memória

        var result = await StartSessionAsync(user, cancellationToken);
        return result with { Response = result.Response with { Reward = reward } };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException(InvalidCredentialsMessage);

        return await StartSessionAsync(user, cancellationToken);
    }

    /// <summary>Troca um refresh token válido por um novo par de tokens (rotação: o antigo é revogado).</summary>
    public async Task<AuthResult> RefreshAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await FindActiveTokenAsync(refreshToken, cancellationToken)
            ?? throw new UnauthorizedException(InvalidSessionMessage);

        stored.RevokedAt = timeProvider.GetUtcNow();
        return await StartSessionAsync(stored.User, cancellationToken);
    }

    /// <summary>Revoga o refresh token. Idempotente: token ausente ou já revogado não é erro.</summary>
    public async Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        var stored = await FindActiveTokenAsync(refreshToken, cancellationToken);
        if (stored is null)
            return;

        stored.RevokedAt = timeProvider.GetUtcNow();
        await users.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException(InvalidSessionMessage);
        return ToDto(user);
    }

    private async Task<RefreshToken?> FindActiveTokenAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var stored = await users.GetRefreshTokenAsync(tokens.HashRefreshToken(refreshToken), cancellationToken);
        var isActive = stored is not null
            && stored.RevokedAt is null
            && stored.ExpiresAt > timeProvider.GetUtcNow();

        return isActive ? stored : null;
    }

    private async Task<AuthResult> StartSessionAsync(User user, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var refreshToken = tokens.CreateRefreshToken();
        var expiresAt = now.Add(tokens.RefreshTokenLifetime);

        users.AddRefreshToken(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.HashRefreshToken(refreshToken),
            CreatedAt = now,
            ExpiresAt = expiresAt
        });
        await users.SaveChangesAsync(cancellationToken);

        var accessToken = tokens.CreateAccessToken(user);
        return new AuthResult(
            new AuthResponse(accessToken.Token, accessToken.ExpiresInSeconds, ToDto(user)),
            refreshToken,
            expiresAt);
    }

    private static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? "";

    private static UserDto ToDto(User user)
    {
        var progress = LevelCalculator.GetProgress(user.ExperiencePoints);
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.AvatarUrl,
            user.Element.ToSummaryDto(),
            progress.Level,
            user.ExperiencePoints,
            progress.CurrentLevelXp,
            progress.NextLevelXp,
            progress.ProgressPercent,
            user.CreatedAt);
    }
}
