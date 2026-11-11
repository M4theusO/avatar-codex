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
    bool IsProfilePrivate,
    DateTimeOffset CreatedAt);

/// <summary>
/// Corpo das respostas de login, cadastro e refresh. Reward só vem no cadastro, com a conquista
/// "Primeiro passo"; nas demais é null.
/// </summary>
public record AuthResponse(string AccessToken, int ExpiresIn, UserDto User, RewardResult? Reward = null);

/// <summary>
/// Resultado interno da autenticação. O refresh token só sai daqui para o cookie httpOnly,
/// nunca para o corpo da resposta.
/// </summary>
public record AuthResult(AuthResponse Response, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);

/// <summary>Edição do perfil: por enquanto, só o nome de usuário (o login é pelo email).</summary>
public record UpdateProfileRequest(string Username);

/// <summary>Visibilidade do perfil público. Null = campo ausente, recusado pelo validator.</summary>
public record UpdatePrivacyRequest(bool? IsProfilePrivate);

/// <summary>Confirmação da exclusão da conta: a senha atual, mesmo com a sessão válida.</summary>
public record DeleteAccountRequest(string Password);
