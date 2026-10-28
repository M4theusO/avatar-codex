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

public record ChallengeSeed(
    string Code,
    string Title,
    string Description,
    ChallengeType ChallengeType,
    int TargetValue,
    int ExperienceReward,
    bool IsActive = true);

public record EraSeed(string Name, string Description);

public record TimelineEventSeed(string Title, string Era, string DateLabel, string Description, List<string> Characters);

/// <param name="RequiresBending">Só dobradores pontuam: quem não dobra tem 0, quem dobra tem mais que 0.</param>
public record ComparisonAttributeSeed(string Name, string Description, bool RequiresBending = false);

/// <summary>Valores de 0 a 100 de um personagem, por nome do atributo.</summary>
public record CharacterAttributesSeed(string Character, Dictionary<string, int> Values);

public record QuizOptionSeed(string Text, bool IsCorrect);

public record QuestionSeed(string Text, string? Explanation, List<QuizOptionSeed> Options);

public record QuizSeed(string Title, string Description, string Category, QuizDifficulty Difficulty, List<QuestionSeed> Questions);

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
    public required List<QuizSeed> Quizzes { get; init; }
    public required List<ChallengeSeed> Challenges { get; init; }

    /// <summary>Na ordem cronológica: a posição no arquivo é a ordem de exibição.</summary>
    public required List<EraSeed> Eras { get; init; }

    /// <summary>Na ordem cronológica, entre todas as eras.</summary>
    public required List<TimelineEventSeed> TimelineEvents { get; init; }

    /// <summary>Na ordem de exibição.</summary>
    public required List<ComparisonAttributeSeed> ComparisonAttributes { get; init; }

    public required List<CharacterAttributesSeed> CharacterAttributes { get; init; }

    public static ContentSeedData Load() => new()
    {
        Elements = Read<ElementSeed>("elements.json"),
        BendingSubtypes = Read<BendingSubtypeSeed>("bending-subtypes.json"),
        Nations = Read<NationSeed>("nations.json"),
        Series = Read<SeriesSeed>("series.json"),
        Abilities = Read<AbilitySeed>("abilities.json"),
        Characters = Read<CharacterSeed>("characters.json"),
        Achievements = Read<AchievementSeed>("achievements.json"),
        Quizzes = Read<QuizSeed>("quizzes.json"),
        Challenges = Read<ChallengeSeed>("challenges.json"),
        Eras = Read<EraSeed>("eras.json"),
        TimelineEvents = Read<TimelineEventSeed>("timeline.json"),
        ComparisonAttributes = Read<ComparisonAttributeSeed>("comparison-attributes.json"),
        CharacterAttributes = Read<CharacterAttributesSeed>("character-attributes.json")
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

        UniqueNames(Challenges.Select(c => c.Code), "desafio (código)", errors);
        foreach (var challenge in Challenges)
        {
            var owner = $"Desafio '{challenge.Code}'";
            if (challenge.ExperienceReward <= 0)
                errors.Add($"{owner} deve dar XP (experienceReward > 0).");
            if (challenge.TargetValue <= 0)
                errors.Add($"{owner} precisa de targetValue > 0.");
            if (challenge.ChallengeType == ChallengeType.QuizScoreAtLeast && challenge.TargetValue > 100)
                errors.Add($"{owner} é uma porcentagem: targetValue deve ser no máximo 100.");
        }
        if (!Challenges.Any(c => c.IsActive))
            errors.Add("É preciso pelo menos um desafio ativo para sortear os desafios do dia.");

        var eras = UniqueNames(Eras.Select(e => e.Name), "era", errors);
        UniqueNames(TimelineEvents.Select(e => e.Title), "evento da linha do tempo", errors);
        var eraOrder = Eras.Select((era, index) => (era.Name, index)).DistinctBy(e => e.Name).ToDictionary(e => e.Name, e => e.index);
        var previousEra = -1;
        foreach (var timelineEvent in TimelineEvents)
        {
            var owner = $"Evento '{timelineEvent.Title}'";
            CheckReference(eras, timelineEvent.Era, owner, "era", errors);
            timelineEvent.Characters.ForEach(c => CheckReference(characters, c, owner, "personagem", errors));
            if (timelineEvent.Characters.Distinct().Count() != timelineEvent.Characters.Count)
                errors.Add($"{owner} repete um personagem.");

            // Os eventos são cronológicos: uma era nunca pode voltar depois que a seguinte começou.
            if (eraOrder.TryGetValue(timelineEvent.Era, out var era))
            {
                if (era < previousEra)
                    errors.Add($"{owner} está fora da ordem das eras ('{timelineEvent.Era}' depois de uma era posterior).");
                previousEra = Math.Max(previousEra, era);
            }
        }

        ValidateComparison(characters, errors);

        UniqueNames(Quizzes.Select(q => q.Title), "quiz", errors);
        foreach (var quiz in Quizzes)
        {
            if (quiz.Questions.Count == 0)
                errors.Add($"Quiz '{quiz.Title}' não tem perguntas.");

            foreach (var (question, index) in quiz.Questions.Select((q, i) => (q, i + 1)))
            {
                var owner = $"Quiz '{quiz.Title}', pergunta {index}";
                if (question.Options.Count < 2)
                    errors.Add($"{owner} precisa de pelo menos 2 opções.");
                if (question.Options.Count(o => o.IsCorrect) != 1)
                    errors.Add($"{owner} deve ter exatamente uma opção correta.");
            }
        }

        return errors;
    }

    /// <summary>Cada personagem listado tem todos os atributos, de 0 a 100 (RN-11: são estimativas, não cânone).</summary>
    private void ValidateComparison(HashSet<string> characters, List<string> errors)
    {
        var attributes = UniqueNames(ComparisonAttributes.Select(a => a.Name), "atributo de comparação", errors);
        UniqueNames(CharacterAttributes.Select(c => c.Character), "personagem nos atributos", errors);
        var benders = Characters.Where(c => c.IsBender).Select(c => c.Name).ToHashSet();

        foreach (var entry in CharacterAttributes)
        {
            var owner = $"Atributos de '{entry.Character}'";
            CheckReference(characters, entry.Character, owner, "personagem", errors);

            foreach (var (name, value) in entry.Values)
            {
                CheckReference(attributes, name, owner, "atributo", errors);
                if (value is < 0 or > 100)
                    errors.Add($"{owner}: '{name}' deve estar entre 0 e 100.");
            }

            foreach (var missing in attributes.Where(a => !entry.Values.ContainsKey(a)))
                errors.Add($"{owner}: falta o atributo '{missing}'.");

            foreach (var attribute in ComparisonAttributes.Where(a => a.RequiresBending))
            {
                if (!entry.Values.TryGetValue(attribute.Name, out var value) || !characters.Contains(entry.Character))
                    continue;
                if (benders.Contains(entry.Character) != value > 0)
                    errors.Add($"{owner}: '{attribute.Name}' deve ser 0 para quem não dobra e maior que 0 para dobradores.");
            }
        }
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
