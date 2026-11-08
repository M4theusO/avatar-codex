namespace AvatarCodex.Application.DTOs;

/// <summary>Filtros de GET /api/ranking: período ("all" ou "month"), elemento opcional e página.</summary>
public class RankingQueryParameters
{
    public string Period { get; set; } = "all";
    public int? ElementId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Linha do ranking. Só dados públicos do jogo (nome de usuário, nível, XP e elemento): nada de email ou id.
/// Experience é o XP do período (total no geral, do mês no mensal); Level é sempre o nível atual.
/// </summary>
public record RankingEntryDto(
    int Position,
    string Username,
    int Level,
    int Experience,
    ElementSummaryDto? Element,
    bool IsCurrentUser);

public record RankingPositionDto(int Position, int Experience);

/// <param name="PeriodStart">Início do mês (UTC) no ranking mensal; null no geral.</param>
/// <param name="PeriodEnd">Início do mês seguinte (UTC), quando o mensal recomeça; null no geral.</param>
/// <param name="CurrentUser">Posição de quem consulta, ou null quando não entra neste ranking.</param>
public record RankingDto(
    string Period,
    DateTimeOffset? PeriodStart,
    DateTimeOffset? PeriodEnd,
    int? ElementId,
    PagedResult<RankingEntryDto> Entries,
    RankingPositionDto? CurrentUser);
