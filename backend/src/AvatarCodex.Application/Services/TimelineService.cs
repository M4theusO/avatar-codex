using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>Linha do tempo (RF-TIME-01): eras e eventos em ordem cronológica, com filtros por era e personagem.</summary>
public class TimelineService(ITimelineRepository timeline, IValidator<TimelineQueryParameters> queryValidator)
{
    public async Task<IReadOnlyList<EraDto>> GetErasAsync(CancellationToken cancellationToken = default)
    {
        var eras = await timeline.GetErasAsync(cancellationToken);
        return [.. eras.Select(e => new EraDto(e.Era.Id, e.Era.Name, e.Era.Description, e.EventCount))];
    }

    /// <summary>
    /// Eventos agrupados por era, na ordem cronológica. Eras sem eventos no filtro ficam de fora; um filtro
    /// sem resultado (ex.: personagem que não aparece na linha do tempo) devolve a lista vazia.
    /// </summary>
    public async Task<IReadOnlyList<TimelineEraDto>> GetTimelineAsync(
        TimelineQueryParameters query, CancellationToken cancellationToken = default)
    {
        await queryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var events = await timeline.GetEventsAsync(query.EraId, query.CharacterId, cancellationToken);

        // Agrupa pelo id: sem tracking, cada evento pode trazer a própria instância da mesma era.
        return [.. events
            .GroupBy(e => e.EraId)
            .Select(g => (Era: g.First().Era, Events: g))
            .OrderBy(g => g.Era.Order)
            .Select(g => new TimelineEraDto(
                g.Era.Id,
                g.Era.Name,
                g.Era.Description,
                [.. g.Events.OrderBy(e => e.Order).Select(e => new TimelineEventDto(
                    e.Id,
                    e.Title,
                    e.Description,
                    e.DateLabel,
                    e.Image.ToDto(),
                    [.. e.Characters.OrderBy(c => c.Name).Select(c => c.ToSummaryDto())]))]))];
    }
}
