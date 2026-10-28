namespace AvatarCodex.Application.DTOs;

/// <summary>Filtros de GET /api/timeline (RF-TIME-01). Os dois são opcionais e podem ser combinados.</summary>
public class TimelineQueryParameters
{
    public int? EraId { get; set; }
    public int? CharacterId { get; set; }
}

public record EraDto(int Id, string Name, string Description, int EventCount);

public record TimelineEventDto(
    int Id,
    string Title,
    string Description,
    string DateLabel,
    ImageDto? Image,
    /// <summary>Personagens relacionados, por nome.</summary>
    IReadOnlyList<CharacterSummaryDto> Characters);

/// <summary>Uma era com os eventos que passaram pelo filtro, já em ordem cronológica.</summary>
public record TimelineEraDto(int Id, string Name, string Description, IReadOnlyList<TimelineEventDto> Events);
