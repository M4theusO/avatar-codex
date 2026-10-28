using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.UnitTests.Comparison;

public class ComparisonServiceTests
{
    private sealed class FakeComparisonRepository : IComparisonRepository
    {
        public List<ComparisonAttribute> Attributes { get; } = [];
        public List<Character> Characters { get; } = [];

        public Task<IReadOnlyList<ComparisonAttribute>> GetAttributesAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ComparisonAttribute>>([.. Attributes.OrderBy(a => a.Order)]);

        // Devolve em ordem de id, como o banco poderia devolver: o serviço deve respeitar a ordem pedida.
        public Task<IReadOnlyList<Character>> GetCharactersAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Character>>([.. Characters.Where(c => ids.Contains(c.Id)).OrderBy(c => c.Id)]);
    }

    private readonly FakeComparisonRepository _repository = new();
    private readonly ComparisonService _service;

    public ComparisonServiceTests()
    {
        _service = new ComparisonService(_repository, new CharacterComparisonQueryValidator());

        var bending = new ComparisonAttribute { Id = 1, Name = "Domínio elemental", Description = "-", Order = 2 };
        var strategy = new ComparisonAttribute { Id = 2, Name = "Estratégia", Description = "-", Order = 1 };
        var agility = new ComparisonAttribute { Id = 3, Name = "Agilidade", Description = "-", Order = 3 };
        _repository.Attributes.AddRange([bending, strategy, agility]);

        _repository.Characters.AddRange(
        [
            Character(1, "Aang", (bending, 95), (strategy, 65), (agility, 80)),
            Character(2, "Sokka", (bending, 0), (strategy, 92), (agility, 80)),
            Character(3, "Sem estimativas")
        ]);
    }

    private static Character Character(int id, string name, params (ComparisonAttribute Attribute, int Value)[] values) => new()
    {
        Id = id,
        Name = name,
        Description = "-",
        Biography = "-",
        AttributeValues = [.. values.Select(v => new CharacterAttributeValue
        {
            CharacterId = id, ComparisonAttributeId = v.Attribute.Id, ComparisonAttribute = v.Attribute, Value = v.Value
        })]
    };

    private Task<CharacterComparisonDto> Compare(string? ids) =>
        _service.CompareAsync(new CharacterComparisonQuery { Ids = ids });

    [Fact]
    public async Task Compare_KeepsRequestedOrderAndAttributeOrder()
    {
        var result = await Compare("2,1");

        Assert.Equal(["Sokka", "Aang"], result.Characters.Select(c => c.Name));
        Assert.Equal(["Estratégia", "Domínio elemental", "Agilidade"], result.Attributes.Select(a => a.Name));
        Assert.Equal([92, 65], result.Attributes[0].Values);
        Assert.Equal([0, 95], result.Attributes[1].Values);
    }

    [Fact]
    public async Task Compare_LeaderIsTheHighestValueAndNobodyOnTie()
    {
        var result = await Compare("1,2");

        Assert.Equal(2, result.Attributes.Single(a => a.Name == "Estratégia").LeaderId);
        Assert.Equal(1, result.Attributes.Single(a => a.Name == "Domínio elemental").LeaderId);
        Assert.Null(result.Attributes.Single(a => a.Name == "Agilidade").LeaderId);
    }

    [Fact]
    public async Task Compare_CharacterWithoutValues_HasNullValuesAndNoLeader()
    {
        var result = await Compare("1,3");

        Assert.All(result.Attributes, a =>
        {
            Assert.Null(a.Values[1]);
            Assert.Null(a.LeaderId);
        });
    }

    [Fact]
    public async Task Compare_UnknownCharacter_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => Compare("1,99"));

        Assert.Contains("99", error.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1,2,3")]
    [InlineData("1,abc")]
    [InlineData("0,2")]
    [InlineData("1,-2")]
    [InlineData("1,1")]
    [InlineData("1,,2")]
    public async Task Compare_InvalidIds_ThrowValidation(string? ids)
    {
        await Assert.ThrowsAsync<ValidationException>(() => Compare(ids));
    }

    [Fact]
    public async Task Compare_SameCharacterTwice_ExplainsTheProblem()
    {
        var error = await Assert.ThrowsAsync<ValidationException>(() => Compare("1, 1"));

        Assert.Equal("Escolha dois personagens diferentes.", Assert.Single(error.Errors).ErrorMessage);
    }

    [Fact]
    public async Task Compare_AcceptsSpacesAroundIds()
    {
        var result = await Compare(" 1 , 2 ");

        Assert.Equal([1, 2], result.Characters.Select(c => c.Id));
    }
}
