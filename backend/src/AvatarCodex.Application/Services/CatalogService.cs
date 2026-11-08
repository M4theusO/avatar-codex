using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Exceptions;

namespace AvatarCodex.Application.Services;

/// <summary>Dados de referência do universo: elementos, habilidades e séries.</summary>
public class CatalogService(ICatalogRepository repository, IUserCollectionRepository collection)
{
    public async Task<IReadOnlyList<ElementDto>> GetElementsAsync(CancellationToken cancellationToken = default)
    {
        var elements = await repository.GetElementsAsync(cancellationToken);
        return [.. elements.Select(e => e.ToDto())];
    }

    public async Task<IReadOnlyList<AbilityListItemDto>> GetAbilitiesAsync(
        int? elementId, CancellationToken cancellationToken = default)
    {
        var abilities = await repository.GetAbilitiesAsync(elementId, cancellationToken);
        return [.. abilities.Select(a => a.Ability.ToListItemDto(a.CharacterCount))];
    }

    /// <param name="userId">Usuário autenticado (token opcional): preenche isFavorite/isDiscovered dos personagens.</param>
    public async Task<AbilityDetailsDto> GetAbilityByIdAsync(
        int id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var ability = await repository.GetAbilityDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Habilidade", id);

        var details = ability.ToDetailsDto();
        var marks = await collection.GetMarksOrNoneAsync(userId, [.. details.Characters.Select(c => c.Id)], cancellationToken);
        return details with { Characters = [.. details.Characters.Select(marks.Apply)] };
    }

    public async Task<IReadOnlyList<SeriesDto>> GetSeriesAsync(CancellationToken cancellationToken = default)
    {
        var series = await repository.GetSeriesAsync(cancellationToken);
        return [.. series.Select(s => s.ToDto())];
    }
}
