using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.UnitTests.Dashboard;

public class DashboardRulesTests
{
    private static QuizListItemDto Quiz(int id, int? bestPercentage = null) => new(
        id,
        $"Quiz {id}",
        "Descrição",
        "Geral",
        QuizDifficulty.Easy,
        null,
        10,
        bestPercentage is null ? null : new QuizBestResultDto(bestPercentage.Value / 10, 10, bestPercentage.Value, 1));

    private static UserAchievementDto Achievement(int id, int progress, int target, bool unlocked = false) => new(
        id, $"Conquista {id}", "Descrição", "award", 10, unlocked, unlocked ? DateTimeOffset.UtcNow : null, progress, target);

    [Fact]
    public void FeaturedQuiz_PrefersTheFirstQuizNotCompletedYet()
    {
        var featured = DashboardRules.FeaturedQuiz([Quiz(1, 100), Quiz(2, 40), Quiz(3), Quiz(4)]);

        Assert.Equal(3, featured!.Id);
    }

    [Fact]
    public void FeaturedQuiz_AllCompleted_PicksTheClosestToAPerfectScore()
    {
        var featured = DashboardRules.FeaturedQuiz([Quiz(1, 100), Quiz(2, 40), Quiz(3, 90), Quiz(4, 90)]);

        // Empate entre 3 e 4: vale a ordem do catálogo.
        Assert.Equal(3, featured!.Id);
    }

    [Fact]
    public void FeaturedQuiz_AllPerfectOrNoQuizzes_ReturnsNull()
    {
        Assert.Null(DashboardRules.FeaturedQuiz([Quiz(1, 100), Quiz(2, 100)]));
        Assert.Null(DashboardRules.FeaturedQuiz([]));
    }

    [Fact]
    public void NextAchievement_IsTheLockedOneClosestToItsTarget()
    {
        var next = DashboardRules.NextAchievement([
            Achievement(1, 1, 1, unlocked: true),
            Achievement(2, 3, 10),
            Achievement(3, 3, 5),
            Achievement(4, 0, 4)
        ]);

        Assert.Equal(3, next!.Id);
    }

    [Fact]
    public void NextAchievement_NoProgressAnywhere_FollowsCatalogOrder()
    {
        var next = DashboardRules.NextAchievement([Achievement(5, 0, 10), Achievement(6, 0, 3)]);

        Assert.Equal(5, next!.Id);
    }

    [Fact]
    public void NextAchievement_AllUnlocked_ReturnsNull()
    {
        Assert.Null(DashboardRules.NextAchievement([Achievement(1, 1, 1, unlocked: true)]));
    }

    [Fact]
    public void PickDaily_IsStableForTheSameUserAndDay()
    {
        var candidates = Enumerable.Range(1, 37).ToList();
        var userId = Guid.NewGuid();
        var day = new DateOnly(2026, 10, 27);

        var first = DashboardRules.PickDaily(candidates, userId, day, 4);
        var second = DashboardRules.PickDaily(candidates, userId, day, 4);

        Assert.Equal(4, first.Count);
        Assert.Equal(4, first.Distinct().Count());
        Assert.All(first, id => Assert.Contains(id, candidates));
        Assert.Equal(first, second);
    }

    [Fact]
    public void PickDaily_RotatesAcrossDays()
    {
        var candidates = Enumerable.Range(1, 37).ToList();
        var userId = Guid.NewGuid();
        var start = new DateOnly(2026, 10, 27);

        // Em uma semana, a seleção não pode ser sempre a mesma.
        var picks = Enumerable.Range(0, 7)
            .Select(offset => string.Join(',', DashboardRules.PickDaily(candidates, userId, start.AddDays(offset), 4)))
            .Distinct();

        Assert.True(picks.Count() > 1);
    }

    [Fact]
    public void PickDaily_FewerCandidatesThanRequested_ReturnsAllOfThem()
    {
        var picked = DashboardRules.PickDaily([7, 8], Guid.NewGuid(), new DateOnly(2026, 10, 27), 4);

        Assert.Equal([7, 8], picked.Order());
        Assert.Empty(DashboardRules.PickDaily(Array.Empty<int>(), Guid.NewGuid(), new DateOnly(2026, 10, 27), 4));
    }
}
