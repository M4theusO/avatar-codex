using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Validators;

namespace AvatarCodex.UnitTests.Characters;

public class CharacterQueryParametersValidatorTests
{
    private readonly CharacterQueryParametersValidator _validator = new();

    [Fact]
    public void Defaults_AreValid()
    {
        Assert.True(_validator.Validate(new CharacterQueryParameters()).IsValid);
    }

    [Theory]
    [InlineData("avatar")]
    [InlineData("Bender")]
    [InlineData("NONBENDER")]
    public void Type_AcceptsKnownValuesIgnoringCase(string type)
    {
        Assert.True(_validator.Validate(new CharacterQueryParameters { Type = type }).IsValid);
    }

    [Theory]
    [InlineData(nameof(CharacterQueryParameters.Page), 0)]
    [InlineData(nameof(CharacterQueryParameters.PageSize), 0)]
    [InlineData(nameof(CharacterQueryParameters.PageSize), 51)]
    public void PagingOutOfRange_IsInvalid(string property, int value)
    {
        var query = new CharacterQueryParameters();
        typeof(CharacterQueryParameters).GetProperty(property)!.SetValue(query, value);

        var result = _validator.Validate(query);

        Assert.Contains(result.Errors, e => e.PropertyName == property);
    }

    [Fact]
    public void NumericEnumValue_IsInvalid()
    {
        // Enum.TryParse aceitaria "0"; a API só aceita os nomes documentados.
        var result = _validator.Validate(new CharacterQueryParameters { Type = "0", Status = "1", Sort = "2" });

        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void SearchLongerThanLimit_IsInvalid()
    {
        var result = _validator.Validate(new CharacterQueryParameters { Search = new string('a', 101) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CharacterQueryParameters.Search));
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("DESC")]
    public void Order_AcceptsAscAndDesc(string order)
    {
        Assert.True(_validator.Validate(new CharacterQueryParameters { Order = order }).IsValid);
    }
}
