using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

/// <summary>Atividades do usuário dentro de um dia (UTC), medidas pelos desafios.</summary>
public record DailyActivity(
    int CharactersDiscovered,
    int NationsDiscovered,
    int QuizzesCompleted,
    /// <summary>Melhor porcentagem de acertos numa tentativa do dia; 0 sem tentativas.</summary>
    int BestQuizPercentage);

public interface IChallengeRepository
{
    /// <summary>Modelos ativos, por id (a ordem fixa deixa o sorteio do dia determinístico).</summary>
    Task<IReadOnlyList<ChallengeTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken);

    /// <summary>Desafios do dia com o Template carregado, por id.</summary>
    Task<IReadOnlyList<DailyChallenge>> GetForDateAsync(DateOnly date, CancellationToken cancellationToken);

    /// <summary>
    /// Cria os desafios do dia. Se outra requisição criou os mesmos ao mesmo tempo, a unicidade
    /// (modelo, data) faz esta gravação ser ignorada, sem erro.
    /// </summary>
    Task AddForDateAsync(DateOnly date, IReadOnlyList<int> templateIds, CancellationToken cancellationToken);

    /// <summary>Quais desses desafios o usuário já concluiu: id → data da conclusão.</summary>
    Task<IReadOnlyDictionary<int, DateTimeOffset>> GetCompletedAsync(
        Guid userId, IReadOnlyCollection<int> dailyChallengeIds, CancellationToken cancellationToken);

    /// <summary>Atividades com data em [from, to).</summary>
    Task<DailyActivity> GetActivityAsync(Guid userId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);

    /// <summary>Desafios concluídos, mais recentes primeiro, com DailyChallenge e Template carregados.</summary>
    Task<(IReadOnlyList<UserDailyChallenge> Items, int TotalItems)> GetHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}
