using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;

namespace AvatarCodex.UnitTests.Social;

public class FollowRulesTests
{
    private static User NewUser(string username, string? email = null) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        Email = email ?? $"{username}@example.com",
        PasswordHash = "x"
    };

    [Fact]
    public void AnotherUser_CanBeFollowed()
    {
        Assert.Null(FollowRules.CannotFollowReason(NewUser("katara"), Guid.NewGuid()));
    }

    [Fact]
    public void Yourself_CannotBeFollowed()
    {
        var me = NewUser("aang");

        Assert.Equal(FollowRules.SelfMessage, FollowRules.CannotFollowReason(me, me.Id));
    }

    [Fact]
    public void DemoAccount_CannotBeFollowed()
    {
        var demo = NewUser(DemoAccount.Username, DemoAccount.Email.ToUpperInvariant());

        Assert.Equal(FollowRules.DemoMessage, FollowRules.CannotFollowReason(demo, Guid.NewGuid()));
    }
}
