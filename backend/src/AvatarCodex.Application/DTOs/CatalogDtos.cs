namespace AvatarCodex.Application.DTOs;

public record NationDto(
    int Id,
    string Name,
    string Description,
    ImageDto? SymbolImage,
    ElementSummaryDto? Element,
    int CharacterCount);

public record NationDetailsDto(
    int Id,
    string Name,
    string Description,
    ImageDto? SymbolImage,
    ElementSummaryDto? Element,
    IReadOnlyList<CharacterListItemDto> Characters);

public record BendingSubtypeDto(int Id, string Name, string Description);

public record ElementDto(
    int Id,
    string Name,
    string Description,
    string Color,
    string Icon,
    IReadOnlyList<BendingSubtypeDto> BendingSubtypes);

public record AbilityDto(int Id, string Name, string Description, ElementSummaryDto? Element, ImageDto? Image);

public record SeriesDto(int Id, string Code, string Name);
