using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.Application.Services;

public class CharacterService(ICharacterRepository repository, IValidator<CharacterQueryParameters> validator)
{
    public async Task<PagedResult<CharacterListItemDto>> GetPagedAsync(
        CharacterQueryParameters query, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);

        var filter = new CharacterFilter(
            query.Page,
            query.PageSize,
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.NationId,
            query.ElementId,
            ParseOrNull<CharacterType>(query.Type),
            ParseOrNull<CharacterStatus>(query.Status),
            query.SeriesId,
            ParseOrNull<CharacterSortField>(query.Sort) ?? CharacterSortField.Name,
            string.Equals(query.Order, "desc", StringComparison.OrdinalIgnoreCase));

        var (items, totalItems) = await repository.GetPagedAsync(filter, cancellationToken);

        return new PagedResult<CharacterListItemDto>(
            [.. items.Select(c => c.ToListItemDto())], filter.Page, filter.PageSize, totalItems);
    }

    public async Task<CharacterDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var character = await repository.GetDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Personagem", id);

        return character.ToDetailsDto();
    }

    public async Task<IReadOnlyList<AbilityDto>> GetAbilitiesAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await repository.ExistsAsync(id, cancellationToken))
            throw NotFoundException.For("Personagem", id);

        var abilities = await repository.GetAbilitiesAsync(id, cancellationToken);
        return [.. abilities.Select(a => a.ToDto())];
    }

    private static TEnum? ParseOrNull<TEnum>(string? value) where TEnum : struct, Enum =>
        value is not null && Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : null;
}
