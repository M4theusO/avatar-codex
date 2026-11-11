namespace AvatarCodex.Application.DTOs;

/// <summary>
/// Pessoa numa lista de seguindo/seguidores: só dados públicos do jogo. IsProfilePrivate diz se o nome
/// leva ao perfil (um perfil que ficou privado continua na lista, mas sem link).
/// </summary>
public record FollowUserDto(
    string Username,
    string? AvatarUrl,
    ElementSummaryDto? Element,
    int Level,
    bool IsProfilePrivate,
    DateTimeOffset FollowedAt);
