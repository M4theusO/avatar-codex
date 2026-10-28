using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.DTOs;

/// <summary>Query string de GET /api/characters/compare (RF-COMP-01): ids=1,2.</summary>
public class CharacterComparisonQuery
{
    public string? Ids { get; set; }

    /// <summary>Os ids na ordem informada, ou null se o texto não for uma lista de inteiros separados por vírgula.</summary>
    public int[]? ParseIds()
    {
        if (string.IsNullOrWhiteSpace(Ids))
            return null;

        var parts = Ids.Split(',', StringSplitOptions.TrimEntries);
        var ids = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out ids[i]))
                return null;
        }
        return ids;
    }
}

/// <summary>Informações do personagem que aparecem lado a lado na comparação.</summary>
public record ComparedCharacterDto(
    int Id,
    string Name,
    string Description,
    ImageDto? Image,
    ReferenceDto? Nation,
    ElementSummaryDto? Element,
    bool IsBender,
    bool IsAvatar,
    CharacterStatus Status,
    string? FirstAppearance,
    IReadOnlyList<ReferenceDto> BendingSubtypes,
    IReadOnlyList<AbilityDto> Abilities,
    IReadOnlyList<SeriesDto> Series);

/// <param name="Values">Um valor (0–100) por personagem, na mesma ordem de <c>Characters</c>; null se não houver estimativa.</param>
/// <param name="LeaderId">Personagem com o maior valor; null em caso de empate ou de valor ausente.</param>
public record AttributeComparisonDto(int Id, string Name, string Description, IReadOnlyList<int?> Values, int? LeaderId);

/// <summary>
/// Dois personagens na ordem pedida e os atributos de comparação, na ordem de exibição.
/// Os atributos são estimativas do projeto, não informação canônica (RN-11).
/// </summary>
public record CharacterComparisonDto(
    IReadOnlyList<ComparedCharacterDto> Characters,
    IReadOnlyList<AttributeComparisonDto> Attributes);
