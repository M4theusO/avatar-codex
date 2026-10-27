using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>Descoberta de personagens e nações (RF-DISC, RN-12, DA-01).</summary>
public class DiscoveryService(
    ICharacterRepository characters,
    INationRepository nations,
    IUserCollectionRepository collection,
    ProgressionService progression,
    TimeProvider timeProvider,
    IValidator<PageQueryParameters> pageValidator)
{
    public async Task<DiscoveryResultDto> DiscoverCharacterAsync(
        Guid userId, int characterId, CancellationToken cancellationToken = default)
    {
        var name = await characters.GetNameAsync(characterId, cancellationToken)
            ?? throw NotFoundException.For("Personagem", characterId);

        var reward = await progression.GrantAsync(
            userId,
            ExperienceType.CharacterDiscovered,
            characterId,
            $"Descobriu {name}",
            new UserCharacterDiscovery
            {
                UserId = userId,
                CharacterId = characterId,
                DiscoveredAt = timeProvider.GetUtcNow()
            },
            cancellationToken);

        return new DiscoveryResultDto(reward is not null, reward);
    }

    public async Task<DiscoveryResultDto> DiscoverNationAsync(
        Guid userId, int nationId, CancellationToken cancellationToken = default)
    {
        var name = await nations.GetNameAsync(nationId, cancellationToken)
            ?? throw NotFoundException.For("Nação", nationId);

        var reward = await progression.GrantAsync(
            userId,
            ExperienceType.NationDiscovered,
            nationId,
            $"Descobriu {name}",
            new UserNationDiscovery
            {
                UserId = userId,
                NationId = nationId,
                DiscoveredAt = timeProvider.GetUtcNow()
            },
            cancellationToken);

        return new DiscoveryResultDto(reward is not null, reward);
    }

    public async Task<PagedResult<DiscoveryDto>> GetHistoryAsync(
        Guid userId, PageQueryParameters query, CancellationToken cancellationToken = default)
    {
        await pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var (items, totalItems) = await collection.GetDiscoveriesAsync(userId, query.Page, query.PageSize, cancellationToken);

        return new PagedResult<DiscoveryDto>(
            [.. items.Select(d => d.ToDto())],
            query.Page,
            query.PageSize,
            totalItems);
    }
}
