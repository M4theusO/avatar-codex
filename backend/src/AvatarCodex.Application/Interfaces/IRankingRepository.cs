namespace AvatarCodex.Application.Interfaces;

/// <summary>From/To = mês do ranking mensal (null no geral); ElementId filtra pelo elemento do usuário.</summary>
public record RankingFilter(DateTimeOffset? From, DateTimeOffset? To, int? ElementId);

/// <summary>Score é o XP do período; TotalExperience é o XP total (para o nível).</summary>
public record RankingRow(
    Guid UserId,
    string Username,
    int TotalExperience,
    int Score,
    int? ElementId,
    string? ElementName,
    string? ElementColor,
    string? ElementIcon);

public interface IRankingRepository
{
    /// <summary>Usuários com XP no período, do maior para o menor (empate pelo nome), paginados.</summary>
    Task<(IReadOnlyList<RankingRow> Rows, int TotalItems)> GetPageAsync(
        RankingFilter filter, int skip, int take, CancellationToken cancellationToken);

    /// <summary>Quantos usuários do ranking têm mais XP no período que o valor informado.</summary>
    Task<int> CountAboveAsync(RankingFilter filter, int score, CancellationToken cancellationToken);

    /// <summary>XP do usuário no período, ou null quando ele não entra neste ranking.</summary>
    Task<int?> GetScoreAsync(RankingFilter filter, Guid userId, CancellationToken cancellationToken);
}
