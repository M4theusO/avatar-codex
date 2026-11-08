using AvatarCodex.Application.Services;

namespace AvatarCodex.UnitTests.Profile;

public class AvatarRulesTests
{
    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00 }, "image/jpeg")]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 }, "image/png")]
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x10, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x56 }, "image/webp")]
    public void DetectContentType_KnownSignature_ReturnsTheType(byte[] data, string expected)
    {
        Assert.Equal(expected, AvatarRules.DetectContentType(data));
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 })] // GIF
    [InlineData(new byte[] { 0x3C, 0x73, 0x76, 0x67 })] // "<svg"
    [InlineData(new byte[] { 0x52, 0x49, 0x46, 0x46, 0x10, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45 })] // RIFF de áudio (WAVE)
    public void DetectContentType_OtherContent_ReturnsNull(byte[] data)
    {
        Assert.Null(AvatarRules.DetectContentType(data));
    }

    [Fact]
    public void UrlFor_UsesThePublicIdWithoutDashes()
    {
        var id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

        Assert.Equal("/api/avatars/0f8fad5bd9cb469fa16570867728950e", AvatarRules.UrlFor(id));
    }
}
