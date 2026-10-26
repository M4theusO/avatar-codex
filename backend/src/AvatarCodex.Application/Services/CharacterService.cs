using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.Application.Services;

public class CharacterService(
    ICharacterRepository repository,
    IUserCollectionRepository collection,
    IValidator<CharacterQueryParameters> validator)
{
    /// <param name="userId">Usuário autenticado (token opcional): preenche isFavorite/isDiscovered.</param>
    public async Task<PagedResult<CharacterListItemDto>> GetPagedAsync(
        CharacterQueryParameters query, Guid? userId = null, CancellationToken cancellationToken = default)
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
        var marks = await collection.GetMarksOrNoneAsync(userId, [.. items.Select(c => c.Id)], cancellationToken);

        return new PagedResult<CharacterListItemDto>(
            [.. items.Select(c => marks.Apply(c.ToListItemDto()))], filter.Page, filter.PageSize, totalItems);
    }

    public async Task<CharacterDetailsDto> GetByIdAsync(
        int id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var character = await repository.GetDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Personagem", id);

        var marks = await collection.GetMarksOrNoneAsync(userId, [id], cancellationToken);
        return marks.Apply(character.ToDetailsDto());
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
