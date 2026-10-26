using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.UnitTests.Fakes;
using FluentValidation;

namespace AvatarCodex.UnitTests.Progress;

public class LevelAndExperienceTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 250)]
    [InlineData(4, 450)]
    [InlineData(5, 700)]
    [InlineData(10, 2_700)]
    public void MinimumXpFor_FollowsRn07(int level, int expectedXp)
    {
        Assert.Equal(expectedXp, LevelCalculator.MinimumXpFor(level));
    }

    [Theory]
    [InlineData(0, 1, 0, 100, 0)]
    [InlineData(50, 1, 0, 100, 50)]
    [InlineData(99, 1, 0, 100, 99)]
    [InlineData(100, 2, 100, 250, 0)]
    [InlineData(175, 2, 100, 250, 50)]
    [InlineData(2_699, 9, 2_200, 2_700, 99)]
    [InlineData(2_700, 10, 2_700, 3_250, 0)]
    public void GetProgress_ReturnsLevelBoundsAndPercent(
        int xp, int level, int currentLevelXp, int nextLevelXp, int percent)
    {
        Assert.Equal(new LevelProgress(level, currentLevelXp, nextLevelXp, percent), LevelCalculator.GetProgress(xp));
    }

    [Fact]
    public void GetLevel_IsAlwaysConsistentWithProgress()
    {
        for (var xp = 0; xp <= 10_000; xp += 7)
        {
            var progress = LevelCalculator.GetProgress(xp);
            Assert.InRange(xp, progress.CurrentLevelXp, progress.NextLevelXp - 1);
            Assert.InRange(progress.ProgressPercent, 0, 99);
        }
    }

    [Fact]
    public async Task Reward_CarriesLevelProgressForTheXpBar()
    {
        var repository = new FakeProgressionRepository();
        var userId = Guid.NewGuid();
        repository.TotalXp[userId] = 165;
        var progression = new ProgressionService(repository, TimeProvider.System);

        var reward = await progression.GrantAsync(userId, ExperienceType.CharacterDiscovered, 1, "Descobriu Aang", new object());

        Assert.Equal((175, 2, 100, 250, 50), (reward!.TotalXp, reward.Level, reward.CurrentLevelXp, reward.NextLevelXp, reward.LevelProgress));
        Assert.False(reward.LeveledUp);
    }

    [Fact]
    public async Task ExperienceHistory_MapsTransactionsNewestFirst()
    {
        var collection = new FakeUserCollectionRepository();
        var userId = Guid.NewGuid();
        var start = new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
        collection.Experience.Add(Transaction(userId, 1, "Descobriu Aang", start));
        collection.Experience.Add(Transaction(userId, 2, "Descobriu Zuko", start.AddMinutes(5)));
        collection.Experience.Add(Transaction(Guid.NewGuid(), 3, "Outro usuário", start));
        var service = new UserService(collection, new PageQueryParametersValidator());

        var history = await service.GetExperienceHistoryAsync(userId, new PageQueryParameters());

        Assert.Equal(2, history.TotalItems);
        Assert.Equal(["Descobriu Zuko", "Descobriu Aang"], history.Items.Select(t => t.Description));
        Assert.All(history.Items, t => Assert.Equal(10, t.Amount));
    }

    [Fact]
    public async Task ExperienceHistory_InvalidPage_ThrowsValidation()
    {
        var service = new UserService(new FakeUserCollectionRepository(), new PageQueryParametersValidator());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetExperienceHistoryAsync(Guid.NewGuid(), new PageQueryParameters { Page = 0 }));
    }

    private static ExperienceTransaction Transaction(Guid userId, int sourceId, string description, DateTimeOffset at) => new()
    {
        Id = sourceId,
        UserId = userId,
        Amount = 10,
        Type = ExperienceType.CharacterDiscovered,
        SourceId = sourceId,
        Description = description,
        CreatedAt = at
    };
}
