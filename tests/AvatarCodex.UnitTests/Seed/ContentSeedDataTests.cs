using AvatarCodex.Domain.Enums;
using AvatarCodex.Infrastructure.Data.Seed;

namespace AvatarCodex.UnitTests.Seed;

public class ContentSeedDataTests
{
    [Fact]
    public void Load_ReadsAllSeedFiles()
    {
        var data = ContentSeedData.Load();

        Assert.Equal(4, data.Elements.Count);
        Assert.NotEmpty(data.BendingSubtypes);
        Assert.NotEmpty(data.Nations);
        Assert.NotEmpty(data.Series);
        Assert.NotEmpty(data.Abilities);
        Assert.NotEmpty(data.Characters);
        Assert.Contains(data.Achievements, a => a.Code == "FIRST_STEP");
        Assert.All(data.Quizzes, q => Assert.Equal(10, q.Questions.Count));
        // Todo personagem do seed pode ser comparado.
        Assert.Equal(data.Characters.Select(c => c.Name).Order(), data.CharacterAttributes.Select(c => c.Character).Order());
    }

    [Fact]
    public void Validate_SeedFilesHaveNoErrors()
    {
        var errors = ContentSeedData.Load().Validate();

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ReportsUnknownReferenceAndDuplicatedName()
    {
        var data = MinimalData(
            Character("Aang", element: "Ar", relationships: [new RelationshipSeed("Ninguém", RelationshipType.Friend, null)]),
            Character("Aang", element: "Ar"));

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("Nome de personagem duplicado: 'Aang'"));
        Assert.Contains(errors, e => e.Contains("personagem relacionado inexistente: 'Ninguém'"));
    }

    [Fact]
    public void Validate_ReportsBenderWithoutElement()
    {
        var data = MinimalData(Character("Sem Elemento", element: null, isBender: true));

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("deve ter elemento"));
    }

    [Fact]
    public void Validate_ReportsSelfRelationship()
    {
        var data = MinimalData(
            Character("Aang", element: "Ar", relationships: [new RelationshipSeed("Aang", RelationshipType.Friend, null)]));

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("consigo mesmo"));
    }

    [Fact]
    public void Validate_ReportsInvalidAchievements()
    {
        var data = MinimalData();
        data.Achievements.AddRange(
        [
            Achievement("CURIOUS", AchievementRequirementType.CharactersDiscovered, value: 0, xp: 50),
            Achievement("CURIOUS", AchievementRequirementType.AllNationsDiscovered, value: 5, xp: 0)
        ]);

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("conquista (código) duplicado: 'CURIOUS'"));
        Assert.Contains(errors, e => e.Contains("precisa de requirementValue > 0"));
        Assert.Contains(errors, e => e.Contains("não usa requirementValue"));
        Assert.Contains(errors, e => e.Contains("deve dar XP"));
    }

    [Fact]
    public void Validate_ReportsQuestionsWithoutExactlyOneCorrectOption()
    {
        var data = MinimalData();
        data.Quizzes.Add(new QuizSeed("Quiz", "Descrição", "Personagens", QuizDifficulty.Easy,
        [
            new QuestionSeed("Duas corretas?", null, [new("A", true), new("B", true)]),
            new QuestionSeed("Nenhuma correta?", null, [new("A", false), new("B", false)]),
            new QuestionSeed("Uma opção só?", null, [new("A", true)])
        ]));

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("pergunta 1 deve ter exatamente uma opção correta"));
        Assert.Contains(errors, e => e.Contains("pergunta 2 deve ter exatamente uma opção correta"));
        Assert.Contains(errors, e => e.Contains("pergunta 3 precisa de pelo menos 2 opções"));
    }

    [Fact]
    public void Validate_ReportsInvalidChallenges()
    {
        var data = MinimalData();
        data.Challenges.AddRange(
        [
            new ChallengeSeed("QUIZ", "Quiz", "Descrição", ChallengeType.QuizScoreAtLeast, 120, 30, IsActive: false),
            new ChallengeSeed("QUIZ", "Quiz", "Descrição", ChallengeType.CompleteQuizzes, 0, 0, IsActive: false)
        ]);

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("desafio (código) duplicado: 'QUIZ'"));
        Assert.Contains(errors, e => e.Contains("no máximo 100"));
        Assert.Contains(errors, e => e.Contains("precisa de targetValue > 0"));
        Assert.Contains(errors, e => e.Contains("deve dar XP"));
        Assert.Contains(errors, e => e.Contains("pelo menos um desafio ativo"));
    }

    [Fact]
    public void Validate_ReportsInvalidTimeline()
    {
        var data = MinimalData(Character("Aang", element: "Ar"));
        data.Eras.AddRange([new EraSeed("Antes", "-"), new EraSeed("Depois", "-"), new EraSeed("Antes", "-")]);
        data.TimelineEvents.AddRange(
        [
            new TimelineEventSeed("Começo", "Depois", "1 AG", "-", ["Aang", "Aang"]),
            new TimelineEventSeed("Volta no tempo", "Antes", "0 AG", "-", ["Ninguém"]),
            new TimelineEventSeed("Sem era", "Inexistente", "?", "-", [])
        ]);

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("era duplicado: 'Antes'"));
        Assert.Contains(errors, e => e.Contains("'Começo' repete um personagem"));
        Assert.Contains(errors, e => e.Contains("'Volta no tempo' está fora da ordem das eras"));
        Assert.Contains(errors, e => e.Contains("personagem inexistente: 'Ninguém'"));
        Assert.Contains(errors, e => e.Contains("era inexistente: 'Inexistente'"));
    }

    [Fact]
    public void Validate_ReportsInvalidComparisonAttributes()
    {
        var data = MinimalData(Character("Aang", element: "Ar"), Character("Sokka", element: null));
        data.ComparisonAttributes.AddRange(
        [
            new ComparisonAttributeSeed("Domínio elemental", "-", RequiresBending: true),
            new ComparisonAttributeSeed("Estratégia", "-")
        ]);
        data.CharacterAttributes.AddRange(
        [
            new CharacterAttributesSeed("Aang", new() { ["Domínio elemental"] = 0, ["Estratégia"] = 101 }),
            new CharacterAttributesSeed("Sokka", new() { ["Domínio elemental"] = 10, ["Força"] = 50 }),
            new CharacterAttributesSeed("Ninguém", new() { ["Domínio elemental"] = 0, ["Estratégia"] = 50 }),
            new CharacterAttributesSeed("Aang", new() { ["Domínio elemental"] = 90, ["Estratégia"] = 60 })
        ]);

        var errors = data.Validate();

        Assert.Contains(errors, e => e.Contains("personagem nos atributos duplicado: 'Aang'"));
        Assert.Contains(errors, e => e.Contains("Atributos de 'Aang': 'Estratégia' deve estar entre 0 e 100"));
        Assert.Contains(errors, e => e.Contains("Atributos de 'Aang': 'Domínio elemental' deve ser 0 para quem não dobra"));
        Assert.Contains(errors, e => e.Contains("Atributos de 'Sokka': 'Domínio elemental' deve ser 0 para quem não dobra"));
        Assert.Contains(errors, e => e.Contains("Atributos de 'Sokka': falta o atributo 'Estratégia'"));
        Assert.Contains(errors, e => e.Contains("atributo inexistente: 'Força'"));
        Assert.Contains(errors, e => e.Contains("personagem inexistente: 'Ninguém'"));
    }

    private static AchievementSeed Achievement(string code, AchievementRequirementType type, int value, int xp) =>
        new(code, code, "Descrição", "award", type, value, xp);

    private static ContentSeedData MinimalData(params CharacterSeed[] characters) => new()
    {
        Elements = [new ElementSeed("Ar", "Elemento do ar", "#D5C7A2", "wind")],
        BendingSubtypes = [],
        Nations = [],
        Series = [],
        Abilities = [],
        Characters = [.. characters],
        Achievements = [],
        Quizzes = [],
        Challenges = [],
        Eras = [],
        TimelineEvents = [],
        ComparisonAttributes = [],
        CharacterAttributes = []
    };

    private static CharacterSeed Character(
        string name, string? element, bool? isBender = null, List<RelationshipSeed>? relationships = null) =>
        new(name, [], "Descrição", "Biografia", Nation: null, element, isBender ?? element is not null,
            IsAvatar: false, CharacterStatus.Alive, FirstAppearance: null, [], [], [], relationships ?? []);
}
