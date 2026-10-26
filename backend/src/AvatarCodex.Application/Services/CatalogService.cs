using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;

namespace AvatarCodex.Application.Services;

/// <summary>Dados de referência do universo: elementos, habilidades e séries.</summary>
public class CatalogService(ICatalogRepository repository)
{
    public async Task<IReadOnlyList<ElementDto>> GetElementsAsync(CancellationToken cancellationToken = default)
    {
        var elements = await repository.GetElementsAsync(cancellationToken);
        return [.. elements.Select(e => e.ToDto())];
    }

    public async Task<IReadOnlyList<AbilityDto>> GetAbilitiesAsync(int? elementId, CancellationToken cancellationToken = default)
    {
        var abilities = await repository.GetAbilitiesAsync(elementId, cancellationToken);
        return [.. abilities.Select(a => a.ToDto())];
    }

    public async Task<IReadOnlyList<SeriesDto>> GetSeriesAsync(CancellationToken cancellationToken = default)
    {
        var series = await repository.GetSeriesAsync(cancellationToken);
        return [.. series.Select(s => s.ToDto())];
    }
}
