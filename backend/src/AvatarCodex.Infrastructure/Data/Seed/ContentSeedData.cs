using System.Text.Json;
using System.Text.Json.Serialization;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Infrastructure.Data.Seed;

public record ElementSeed(string Name, string Description, string Color, string Icon);

public record BendingSubtypeSeed(string Name, string Element, string Description);

public record NationSeed(string Name, string? Element, string Description);

public record SeriesSeed(string Code, string Name);

public record AbilitySeed(string Name, string? Element, string Description);

public record AchievementSeed(
    string Code,
    string Name,
    string Description,
    string Icon,
    AchievementRequirementType RequirementType,
    int RequirementValue,
    int ExperienceReward);

public record RelationshipSeed(string Related, RelationshipType Type, string? Description);

public record CharacterSeed(
    string Name,
    List<string> Aliases,
    string Description,
    string Biography,
    string? Nation,
    string? Element,
    bool IsBender,
    bool IsAvatar,
    CharacterStatus Status,
    string? FirstAppearance,
    List<string> Series,
    List<string> BendingSubtypes,
    List<string> Abilities,
    List<RelationshipSeed> Relationships);

/// <summary>
/// Conteúdo de demonstração lido dos JSON em Data/Seed/Content (embutidos no assembly).
/// As referências entre arquivos são feitas por nome (ou código, no caso das séries).
/// </summary>
public class ContentSeedData
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public required List<ElementSeed> Elements { get; init; }
    public required List<BendingSubtypeSeed> BendingSubtypes { get; init; }
    public required List<NationSeed> Nations { get; init; }
    public required List<SeriesSeed> Series { get; init; }
    public required List<AbilitySeed> Abilities { get; init; }
    public required List<CharacterSeed> Characters { get; init; }
    public required List<AchievementSeed> Achievements { get; init; }

    public static ContentSeedData Load() => new()
    {
        Elements = Read<ElementSeed>("elements.json"),
        BendingSubtypes = Read<BendingSubtypeSeed>("bending-subtypes.json"),
        Nations = Read<NationSeed>("nations.json"),
        Series = Read<SeriesSeed>("series.json"),
        Abilities = Read<AbilitySeed>("abilities.json"),
        Characters = Read<CharacterSeed>("characters.json"),
        Achievements = Read<AchievementSeed>("achievements.json")
    };

    /// <summary>Retorna os problemas encontrados (nomes duplicados ou referências inexistentes).</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        var elements = UniqueNames(Elements.Select(e => e.Name), "elemento", errors);
        var subtypes = UniqueNames(BendingSubtypes.Select(b => b.Name), "subtipo de dobra", errors);
        var nations = UniqueNames(Nations.Select(n => n.Name), "nação", errors);
        var series = UniqueNames(Series.Select(s => s.Code), "série", errors);
        var abilities = UniqueNames(Abilities.Select(a => a.Name), "habilidade", errors);
        var characters = UniqueNames(Characters.Select(c => c.Name), "personagem", errors);

        foreach (var subtype in BendingSubtypes)
            CheckReference(elements, subtype.Element, $"Subtipo '{subtype.Name}'", "elemento", errors);

        foreach (var nation in Nations)
            CheckReference(elements, nation.Element, $"Nação '{nation.Name}'", "elemento", errors);

        foreach (var ability in Abilities)
            CheckReference(elements, ability.Element, $"Habilidade '{ability.Name}'", "elemento", errors);

        foreach (var character in Characters)
        {
            var owner = $"Personagem '{character.Name}'";
            CheckReference(nations, character.Nation, owner, "nação", errors);
            CheckReference(elements, character.Element, owner, "elemento", errors);
            character.Series.ForEach(s => CheckReference(series, s, owner, "série", errors));
            character.BendingSubtypes.ForEach(b => CheckReference(subtypes, b, owner, "subtipo de dobra", errors));
            character.Abilities.ForEach(a => CheckReference(abilities, a, owner, "habilidade", errors));

            foreach (var relationship in character.Relationships)
            {
                CheckReference(characters, relationship.Related, owner, "personagem relacionado", errors);
                if (relationship.Related == character.Name)
                    errors.Add($"{owner} não pode se relacionar consigo mesmo.");
            }

            var duplicatedRelationships = character.Relationships
                .GroupBy(r => (r.Related, r.Type))
                .Where(g => g.Count() > 1);
            foreach (var duplicated in duplicatedRelationships)
                errors.Add($"{owner} tem a relação '{duplicated.Key.Type}' com '{duplicated.Key.Related}' repetida.");

            if (character.IsAvatar && !character.IsBender)
                errors.Add($"{owner} é Avatar, mas não está marcado como dobrador.");
            if (character.IsBender != (character.Element is not null))
                errors.Add($"{owner} deve ter elemento se, e somente se, for dobrador.");
        }

        UniqueNames(Achievements.Select(a => a.Code), "conquista (código)", errors);
        foreach (var achievement in Achievements)
        {
            var owner = $"Conquista '{achievement.Code}'";
            if (achievement.ExperienceReward <= 0)
                errors.Add($"{owner} deve dar XP (experienceReward > 0).");

            // Nesses dois tipos a meta é fixa (conta criada) ou vem do conteúdo (todas as nações).
            var usesValue = achievement.RequirementType is not (
                AchievementRequirementType.AccountCreated or AchievementRequirementType.AllNationsDiscovered);
            if (usesValue && achievement.RequirementValue <= 0)
                errors.Add($"{owner} precisa de requirementValue > 0.");
            if (!usesValue && achievement.RequirementValue != 0)
                errors.Add($"{owner} não usa requirementValue (deve ser 0).");
        }

        return errors;
    }

    private static List<T> Read<T>(string fileName)
    {
        var resourceName = $"seed/{fileName}";
        using var stream = typeof(ContentSeedData).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Arquivo de seed '{resourceName}' não encontrado.");
        return JsonSerializer.Deserialize<List<T>>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Arquivo de seed '{resourceName}' está vazio.");
    }

    private static HashSet<string> UniqueNames(IEnumerable<string> names, string kind, List<string> errors)
    {
        var set = new HashSet<string>();
        foreach (var name in names)
        {
            if (!set.Add(name))
                errors.Add($"Nome de {kind} duplicado: '{name}'.");
        }
        return set;
    }

    private static void CheckReference(HashSet<string> known, string? reference, string owner, string kind, List<string> errors)
    {
        if (reference is not null && !known.Contains(reference))
            errors.Add($"{owner} referencia {kind} inexistente: '{reference}'.");
    }
}
