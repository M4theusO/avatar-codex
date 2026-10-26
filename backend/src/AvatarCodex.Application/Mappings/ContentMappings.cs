using AvatarCodex.Application.DTOs;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.ValueObjects;

namespace AvatarCodex.Application.Mappings;

public static class ContentMappings
{
    public static ImageDto? ToDto(this ImageInfo? image) =>
        image is null ? null : new ImageDto(image.Url, image.SourceUrl, image.Credit);

    public static ElementSummaryDto? ToSummaryDto(this Element? element) =>
        element is null ? null : new ElementSummaryDto(element.Id, element.Name, element.Color, element.Icon);

    public static ReferenceDto? ToReferenceDto(this Nation? nation) =>
        nation is null ? null : new ReferenceDto(nation.Id, nation.Name);

    public static CharacterListItemDto ToListItemDto(this Character character) => new(
        character.Id,
        character.Name,
        character.Description,
        character.Image.ToDto(),
        character.Nation.ToReferenceDto(),
        character.Element.ToSummaryDto(),
        character.IsBender,
        character.IsAvatar,
        character.Status);

    public static CharacterSummaryDto ToSummaryDto(this Character character) => new(
        character.Id,
        character.Name,
        character.Image.ToDto(),
        character.Element.ToSummaryDto());

    public static CharacterDetailsDto ToDetailsDto(this Character character) => new(
        character.Id,
        character.Name,
        character.Aliases,
        character.Description,
        character.Biography,
        character.Image.ToDto(),
        character.Nation.ToReferenceDto(),
        character.Element.ToSummaryDto(),
        character.IsBender,
        character.IsAvatar,
        character.Status,
        character.FirstAppearance,
        [.. character.BendingSubtypes.OrderBy(b => b.Name).Select(b => new ReferenceDto(b.Id, b.Name))],
        [.. character.Abilities.OrderBy(a => a.Name).Select(a => a.ToDto())],
        [.. character.Series.OrderBy(s => s.Id).Select(s => s.ToDto())],
        [.. character.Relationships
            .OrderBy(r => r.Type)
            .ThenBy(r => r.RelatedCharacter.Name)
            .Select(r => new CharacterRelationshipDto(r.RelatedCharacter.ToSummaryDto(), r.Type, r.Description))]);

    public static AbilityDto ToDto(this Ability ability) => new(
        ability.Id,
        ability.Name,
        ability.Description,
        ability.Element.ToSummaryDto(),
        ability.Image.ToDto());

    public static SeriesDto ToDto(this Series series) => new(series.Id, series.Code, series.Name);

    public static ElementDto ToDto(this Element element) => new(
        element.Id,
        element.Name,
        element.Description,
        element.Color,
        element.Icon,
        [.. element.BendingSubtypes.OrderBy(b => b.Name).Select(b => new BendingSubtypeDto(b.Id, b.Name, b.Description))]);

    public static NationDto ToDto(this Nation nation, int characterCount) => new(
        nation.Id,
        nation.Name,
        nation.Description,
        nation.SymbolImage.ToDto(),
        nation.Element.ToSummaryDto(),
        characterCount);

    public static NationDetailsDto ToDetailsDto(this Nation nation) => new(
        nation.Id,
        nation.Name,
        nation.Description,
        nation.SymbolImage.ToDto(),
        nation.Element.ToSummaryDto(),
        [.. nation.Characters.OrderBy(c => c.Name).Select(c => c.ToListItemDto())]);
}
