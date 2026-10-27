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
        Quizzes = []
    };

    private static CharacterSeed Character(
        string name, string? element, bool? isBender = null, List<RelationshipSeed>? relationships = null) =>
        new(name, [], "Descrição", "Biografia", Nation: null, element, isBender ?? element is not null,
            IsAvatar: false, CharacterStatus.Alive, FirstAppearance: null, [], [], [], relationships ?? []);
}
