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

    private static ContentSeedData MinimalData(params CharacterSeed[] characters) => new()
    {
        Elements = [new ElementSeed("Ar", "Elemento do ar", "#D5C7A2", "wind")],
        BendingSubtypes = [],
        Nations = [],
        Series = [],
        Abilities = [],
        Characters = [.. characters]
    };

    private static CharacterSeed Character(
        string name, string? element, bool? isBender = null, List<RelationshipSeed>? relationships = null) =>
        new(name, [], "Descrição", "Biografia", Nation: null, element, isBender ?? element is not null,
            IsAvatar: false, CharacterStatus.Alive, FirstAppearance: null, [], [], [], relationships ?? []);
}
