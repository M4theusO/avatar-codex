using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;

namespace AvatarCodex.UnitTests.Auth;

public class AuthValidatorsTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Register_ValidRequest_Passes()
    {
        Assert.True(_validator.Validate(new RegisterRequest("toph_beifong", "toph@example.com", "Metal2024", "Metal2024")).IsValid);
    }

    [Theory]
    [InlineData("curta1", "A senha deve ter pelo menos 8 caracteres.")]
    [InlineData("somenteletras", "A senha deve ter pelo menos uma letra e um número.")]
    [InlineData("12345678", "A senha deve ter pelo menos uma letra e um número.")]
    public void Register_WeakPassword_Fails(string password, string message)
    {
        var result = _validator.Validate(new RegisterRequest("toph", "toph@example.com", password, password));

        var error = Assert.Single(result.Errors);
        Assert.Equal("Password", error.PropertyName);
        Assert.Equal(message, error.ErrorMessage);
    }

    [Fact]
    public void Register_PasswordsDoNotMatch_Fails()
    {
        var result = _validator.Validate(new RegisterRequest("toph", "toph@example.com", "Metal2024", "Metal2025"));

        Assert.Equal("ConfirmPassword", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("nome com espaço")]
    [InlineData("joão")]
    [InlineData("este_nome_de_usuario_tem_mais_de_30")]
    public void Register_InvalidUsername_Fails(string username)
    {
        var result = _validator.Validate(new RegisterRequest(username, "toph@example.com", "Metal2024", "Metal2024"));

        Assert.Equal("Username", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("sem-arroba")]
    public void Register_InvalidEmail_Fails(string email)
    {
        var result = _validator.Validate(new RegisterRequest("toph", email, "Metal2024", "Metal2024"));

        Assert.Equal("Email", Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(249, 2)]
    [InlineData(250, 3)]
    [InlineData(450, 4)]
    [InlineData(700, 5)]
    [InlineData(2_700, 10)]
    [InlineData(2_699, 9)]
    public void LevelCalculator_FollowsRn07(int experiencePoints, int expectedLevel)
    {
        Assert.Equal(expectedLevel, LevelCalculator.GetLevel(experiencePoints));
    }
}
