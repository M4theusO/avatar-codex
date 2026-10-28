using AvatarCodex.Infrastructure.Security;

namespace AvatarCodex.UnitTests.Auth;

public class JwtOptionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("segredo-curto-demais")]
    public void GetSigningKey_SecretShorterThan256Bits_FailsWithAHelpfulMessage(string secret)
    {
        var error = Assert.Throws<InvalidOperationException>(() => new JwtOptions { Secret = secret }.GetSigningKey());

        Assert.Contains("Jwt:Secret", error.Message);
    }

    [Fact]
    public void GetSigningKey_LongEnoughSecret_ReturnsItsBytes()
    {
        var secret = new string('a', JwtOptions.MinimumSecretBytes);

        Assert.Equal(JwtOptions.MinimumSecretBytes, new JwtOptions { Secret = secret }.GetSigningKey().Length);
    }
}
