namespace AvatarCodex.Application.DTOs;

public record RegisterRequest(string Username, string Email, string Password, string ConfirmPassword);

public record LoginRequest(string Email, string Password);

/// <summary>
/// Resumo do usuário autenticado. Nunca inclui o hash da senha.
/// CurrentLevelXp/NextLevelXp/LevelProgress alimentam a barra de XP sem o frontend conhecer a fórmula.
/// </summary>
public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? AvatarUrl,
    ElementSummaryDto? Element,
    int Level,
    int ExperiencePoints,
    int CurrentLevelXp,
    int NextLevelXp,
    int LevelProgress,
    DateTimeOffset CreatedAt);

/// <summary>Corpo das respostas de login, cadastro e refresh.</summary>
public record AuthResponse(string AccessToken, int ExpiresIn, UserDto User);

/// <summary>
/// Resultado interno da autenticação. O refresh token só sai daqui para o cookie httpOnly,
/// nunca para o corpo da resposta.
/// </summary>
public record AuthResult(AuthResponse Response, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);
