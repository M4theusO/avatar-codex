using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>Favoritos (RF-FAV-01). Adicionar e remover são idempotentes.</summary>
public class FavoriteService(
    ICharacterRepository characters,
    IUserCollectionRepository collection,
    TimeProvider timeProvider,
    IValidator<PageQueryParameters> pageValidator)
{
    public async Task AddAsync(Guid userId, int characterId, CancellationToken cancellationToken = default)
    {
        await EnsureCharacterExistsAsync(characterId, cancellationToken);
        await collection.AddFavoriteAsync(userId, characterId, timeProvider.GetUtcNow(), cancellationToken);
    }

    public async Task RemoveAsync(Guid userId, int characterId, CancellationToken cancellationToken = default)
    {
        await EnsureCharacterExistsAsync(characterId, cancellationToken);
        await collection.RemoveFavoriteAsync(userId, characterId, cancellationToken);
    }

    public async Task<PagedResult<CharacterListItemDto>> GetPagedAsync(
        Guid userId, PageQueryParameters query, CancellationToken cancellationToken = default)
    {
        await pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var (items, totalItems) = await collection.GetFavoritesAsync(userId, query.Page, query.PageSize, cancellationToken);
        var marks = await collection.GetMarksAsync(userId, [.. items.Select(c => c.Id)], cancellationToken);

        return new PagedResult<CharacterListItemDto>(
            [.. items.Select(c => marks.Apply(c.ToListItemDto()))], query.Page, query.PageSize, totalItems);
    }

    private async Task EnsureCharacterExistsAsync(int characterId, CancellationToken cancellationToken)
    {
        if (!await characters.ExistsAsync(characterId, cancellationToken))
            throw NotFoundException.For("Personagem", characterId);
    }
}
