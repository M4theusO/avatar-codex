using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

public interface ITimelineRepository
{
    /// <summary>Eras em ordem cronológica, com a quantidade de eventos de cada uma.</summary>
    Task<IReadOnlyList<(Era Era, int EventCount)>> GetErasAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Eventos em ordem cronológica, com Era e personagens (com Element) carregados. Filtros opcionais:
    /// só os da era e/ou só os que envolvem o personagem.
    /// </summary>
    Task<IReadOnlyList<TimelineEvent>> GetEventsAsync(int? eraId, int? characterId, CancellationToken cancellationToken);
}
