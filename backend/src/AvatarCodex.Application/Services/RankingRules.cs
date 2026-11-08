namespace AvatarCodex.Application.Services;

/// <summary>Regras do ranking (RF-RANK-01). Funções puras, sem banco.</summary>
public static class RankingRules
{
    public const string AllTime = "all";
    public const string Month = "month";

    /// <summary>"all" ou "month", sem diferenciar maiúsculas. monthly = true para o ranking do mês.</summary>
    public static bool TryParsePeriod(string? period, out bool monthly)
    {
        monthly = string.Equals(period, Month, StringComparison.OrdinalIgnoreCase);
        return monthly || string.Equals(period, AllTime, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Mês corrente em UTC: [início do mês, início do mês seguinte).</summary>
    public static (DateTimeOffset Start, DateTimeOffset End) MonthOf(DateTimeOffset now)
    {
        var utc = now.ToUniversalTime();
        var start = new DateTimeOffset(utc.Year, utc.Month, 1, 0, 0, 0, TimeSpan.Zero);
        return (start, start.AddMonths(1));
    }

    /// <summary>
    /// Posições por competição ("1, 2, 2, 4"): empatados dividem a posição e o próximo pula as repetidas.
    /// Assim ninguém fica à frente por um critério arbitrário; entre empatados, a ordem de exibição é pelo nome.
    /// </summary>
    /// <param name="firstPosition">Posição do primeiro item da página (1 + quantos têm mais XP que ele).</param>
    /// <param name="skip">Quantos itens vêm antes desta página.</param>
    /// <param name="scores">XP de cada item da página, em ordem decrescente.</param>
    public static IReadOnlyList<int> Positions(int firstPosition, int skip, IReadOnlyList<int> scores)
    {
        var positions = new int[scores.Count];
        for (var i = 0; i < scores.Count; i++)
            positions[i] = i == 0 ? firstPosition : scores[i] == scores[i - 1] ? positions[i - 1] : skip + i + 1;
        return positions;
    }
}
