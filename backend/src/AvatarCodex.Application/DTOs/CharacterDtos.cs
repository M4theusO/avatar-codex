using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.DTOs;

/// <summary>Parâmetros de listagem recebidos pela query string de GET /api/characters.</summary>
public class CharacterQueryParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? Search { get; set; }
    public int? NationId { get; set; }
    public int? ElementId { get; set; }

    /// <summary>avatar | bender | nonBender</summary>
    public string? Type { get; set; }

    /// <summary>Alive | Deceased | Unknown</summary>
    public string? Status { get; set; }

    public int? SeriesId { get; set; }

    /// <summary>name | nation | element</summary>
    public string? Sort { get; set; }

    /// <summary>asc | desc</summary>
    public string? Order { get; set; }
}

public record CharacterListItemDto(
    int Id,
    string Name,
    string Description,
    ImageDto? Image,
    ReferenceDto? Nation,
    ElementSummaryDto? Element,
    bool IsBender,
    bool IsAvatar,
    CharacterStatus Status);

public record CharacterSummaryDto(int Id, string Name, ImageDto? Image, ElementSummaryDto? Element);

public record CharacterRelationshipDto(CharacterSummaryDto Character, RelationshipType Type, string? Description);

public record CharacterDetailsDto(
    int Id,
    string Name,
    IReadOnlyList<string> Aliases,
    string Description,
    string Biography,
    ImageDto? Image,
    ReferenceDto? Nation,
    ElementSummaryDto? Element,
    bool IsBender,
    bool IsAvatar,
    CharacterStatus Status,
    string? FirstAppearance,
    IReadOnlyList<ReferenceDto> BendingSubtypes,
    IReadOnlyList<AbilityDto> Abilities,
    IReadOnlyList<SeriesDto> Series,
    IReadOnlyList<CharacterRelationshipDto> Relationships);
