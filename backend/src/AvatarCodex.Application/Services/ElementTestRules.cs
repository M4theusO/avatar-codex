using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Services;

public record ElementScore(int ElementId, int Points);

/// <summary>Pontuação do teste de elemento (RF-ELTEST-01). Funções puras, sem banco.</summary>
public static class ElementTestRules
{
    /// <summary>O teste é um só: o XP dele usa sempre esta origem, então só é concedido uma vez (RN-05).</summary>
    public const int ExperienceSourceId = 1;

    /// <summary>
    /// Cada resposta vale um ponto para o elemento da opção escolhida. Em caso de empate, vence o elemento
    /// escolhido na pergunta mais recente entre os empatados: as mesmas respostas dão sempre o mesmo resultado.
    /// </summary>
    /// <param name="questions">Perguntas com as opções carregadas.</param>
    /// <param name="selected">Opção escolhida por pergunta (já validada: uma por pergunta, todas respondidas).</param>
    /// <param name="elementIds">Todos os elementos, na ordem de exibição (os sem pontos aparecem com 0).</param>
    public static (int WinnerId, IReadOnlyList<ElementScore> Scores) Score(
        IReadOnlyList<ElementTestQuestion> questions,
        IReadOnlyDictionary<int, int> selected,
        IReadOnlyList<int> elementIds)
    {
        var picks = questions
            .OrderBy(q => q.Order)
            .Select(q => q.Options.Single(o => o.Id == selected[q.Id]).ElementId)
            .ToList();

        var points = picks.CountBy(id => id).ToDictionary(p => p.Key, p => p.Value);
        var best = points.Values.Max();
        var tied = points.Where(p => p.Value == best).Select(p => p.Key).ToHashSet();
        var winner = picks.Last(tied.Contains);

        return (winner, [.. elementIds.Select(id => new ElementScore(id, points.GetValueOrDefault(id)))]);
    }
}
