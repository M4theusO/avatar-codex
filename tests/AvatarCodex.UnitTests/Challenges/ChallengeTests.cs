using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.UnitTests.Fakes;

namespace AvatarCodex.UnitTests.Challenges;

public class ChallengeRulesTests
{
    private static ChallengeTemplate Template(int id, ChallengeType type, int target = 1, int xp = 30) => new()
    {
        Id = id,
        Code = $"T{id}",
        Title = $"Desafio {id}",
        Description = "Descrição",
        ChallengeType = type,
        TargetValue = target,
        ExperienceReward = xp
    };

    private static readonly IReadOnlyList<ChallengeTemplate> Pool =
    [
        Template(1, ChallengeType.DiscoverCharacters, 3),
        Template(2, ChallengeType.DiscoverCharacters, 5),
        Template(3, ChallengeType.DiscoverNations),
        Template(4, ChallengeType.DiscoverNations, 2),
        Template(5, ChallengeType.CompleteQuizzes),
        Template(6, ChallengeType.CompleteQuizzes, 2),
        Template(7, ChallengeType.QuizScoreAtLeast, 80),
        Template(8, ChallengeType.QuizScoreAtLeast, 100)
    ];

    [Theory]
    [InlineData(ChallengeType.DiscoverCharacters, 3, 2, false)]
    [InlineData(ChallengeType.DiscoverCharacters, 3, 3, true)]
    [InlineData(ChallengeType.DiscoverNations, 1, 1, true)]
    [InlineData(ChallengeType.CompleteQuizzes, 2, 1, false)]
    [InlineData(ChallengeType.QuizScoreAtLeast, 80, 70, false)]
    [InlineData(ChallengeType.QuizScoreAtLeast, 80, 80, true)]
    public void IsMet_ComparesTheDayActivityWithTheTarget(ChallengeType type, int target, int current, bool expected)
    {
        var activity = type switch
        {
            ChallengeType.DiscoverCharacters => new DailyActivity(current, 0, 0, 0),
            ChallengeType.DiscoverNations => new DailyActivity(0, current, 0, 0),
            ChallengeType.CompleteQuizzes => new DailyActivity(0, 0, current, 0),
            _ => new DailyActivity(0, 0, 0, current)
        };

        Assert.Equal(expected, ChallengeRules.IsMet(Template(1, type, target), activity));
    }

    [Fact]
    public void Progress_IsCappedAtTheTarget()
    {
        var template = Template(1, ChallengeType.DiscoverCharacters, 3);

        Assert.Equal(2, ChallengeRules.Progress(template, new DailyActivity(2, 0, 0, 0)));
        Assert.Equal(3, ChallengeRules.Progress(template, new DailyActivity(7, 0, 0, 0)));
    }

    [Fact]
    public void PickForDay_IsDeterministicAndPrefersDifferentTypes()
    {
        var day = new DateOnly(2026, 10, 27);

        var first = ChallengeRules.PickForDay(Pool, day);
        var second = ChallengeRules.PickForDay(Pool, day);

        Assert.Equal(3, first.Count);
        Assert.Equal(first.Select(t => t.Id), second.Select(t => t.Id));
        Assert.Equal(3, first.Select(t => t.ChallengeType).Distinct().Count());
    }

    [Fact]
    public void PickForDay_RotatesAcrossDays()
    {
        var start = new DateOnly(2026, 10, 27);

        var picks = Enumerable.Range(0, 7)
            .Select(offset => string.Join(',', ChallengeRules.PickForDay(Pool, start.AddDays(offset)).Select(t => t.Id)))
            .Distinct();

        Assert.True(picks.Count() > 1);
    }

    [Fact]
    public void PickForDay_FewerTypesThanChallenges_CompletesWithRemainingTemplates()
    {
        IReadOnlyList<ChallengeTemplate> quizOnly =
            [Template(1, ChallengeType.CompleteQuizzes), Template(2, ChallengeType.CompleteQuizzes, 2)];

        Assert.Equal([1, 2], ChallengeRules.PickForDay(quizOnly, new DateOnly(2026, 10, 27)).Select(t => t.Id).Order());
        Assert.Empty(ChallengeRules.PickForDay([], new DateOnly(2026, 10, 27)));
    }

    [Fact]
    public void DayRange_IsTheWholeUtcDay()
    {
        var (from, to) = ChallengeRules.DayRange(new DateOnly(2026, 10, 27));

        Assert.Equal(new DateTimeOffset(2026, 10, 27, 0, 0, 0, TimeSpan.Zero), from);
        Assert.Equal(new DateTimeOffset(2026, 10, 28, 0, 0, 0, TimeSpan.Zero), to);
    }
}

public class DailyChallengeServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    // 23h50 UTC: o próximo reset é daqui a 10 minutos.
    private readonly FixedTimeProvider _time = new(new DateTimeOffset(2026, 10, 27, 23, 50, 0, TimeSpan.Zero));
    private readonly FakeProgressionRepository _progressionRepository = new();
    private readonly FakeChallengeRepository _challenges;
    private readonly DailyChallengeService _service;
    private readonly ProgressionService _progression;

    public DailyChallengeServiceTests()
    {
        _challenges = new FakeChallengeRepository(_progressionRepository);
        _challenges.Templates.AddRange(
        [
            new ChallengeTemplate { Id = 1, Code = "C", Title = "Descubra 3 personagens", Description = "-", ChallengeType = ChallengeType.DiscoverCharacters, TargetValue = 3, ExperienceReward = 30 },
            new ChallengeTemplate { Id = 2, Code = "N", Title = "Explore uma nação", Description = "-", ChallengeType = ChallengeType.DiscoverNations, TargetValue = 1, ExperienceReward = 30 },
            new ChallengeTemplate { Id = 3, Code = "Q", Title = "Acerte 80% de um quiz", Description = "-", ChallengeType = ChallengeType.QuizScoreAtLeast, TargetValue = 80, ExperienceReward = 40 }
        ]);
        _service = new DailyChallengeService(_challenges, _time, new PageQueryParametersValidator());
        var achievements = new FakeAchievementRepository(_progressionRepository);
        _progression = TestProgression.Create(_progressionRepository, achievements, _time, _challenges);
    }

    private DailyChallengeDto Challenge(DailyChallengesDto today, ChallengeType type) =>
        today.Challenges.Single(c => c.Type == type);

    [Fact]
    public async Task GetToday_GeneratesOnceAndShowsProgress()
    {
        _challenges.Activity = new DailyActivity(2, 0, 0, 60);

        var today = await _service.GetTodayAsync(UserId);
        await _service.GetTodayAsync(UserId);

        Assert.Equal(1, _challenges.AddCalls);
        Assert.Equal(new DateOnly(2026, 10, 27), today.Date);
        Assert.Equal(new DateTimeOffset(2026, 10, 28, 0, 0, 0, TimeSpan.Zero), today.ResetsAt);
        Assert.Equal(3, today.Challenges.Count);
        Assert.Equal((2, 3, false), (Challenge(today, ChallengeType.DiscoverCharacters).Progress, Challenge(today, ChallengeType.DiscoverCharacters).Target, Challenge(today, ChallengeType.DiscoverCharacters).IsCompleted));
        Assert.Equal((60, 80), (Challenge(today, ChallengeType.QuizScoreAtLeast).Progress, Challenge(today, ChallengeType.QuizScoreAtLeast).Target));
    }

    [Fact]
    public async Task Progression_CompletesMetChallengesOnceAndGrantsTheirXp()
    {
        _challenges.Activity = new DailyActivity(3, 0, 0, 0);

        var reward = await _progression.GrantAsync(UserId, ExperienceType.CharacterDiscovered, 13, "Descobriu Zuko", new object());

        var completed = Assert.Single(reward!.ChallengesCompleted);
        Assert.Equal(("Descubra 3 personagens", 30), (completed.Title, completed.ExperienceReward));
        Assert.Equal(40, reward.XpGained);
        Assert.Equal(40, reward.TotalXp);
        var transaction = _progressionRepository.Transactions.Single(t => t.Type == ExperienceType.ChallengeCompleted);
        Assert.Equal((completed.Id, 30, "Desafio: Descubra 3 personagens"), (transaction.SourceId, transaction.Amount, transaction.Description));

        // Outro evento no mesmo dia: o desafio já concluído não rende de novo.
        var next = await _progression.GrantAsync(UserId, ExperienceType.CharacterDiscovered, 14, "Descobriu Iroh", new object());
        Assert.Empty(next!.ChallengesCompleted);
        Assert.Single(_progressionRepository.Transactions, t => t.Type == ExperienceType.ChallengeCompleted);

        var today = await _service.GetTodayAsync(UserId);
        var done = Challenge(today, ChallengeType.DiscoverCharacters);
        Assert.True(done.IsCompleted);
        Assert.Equal(_time.GetUtcNow(), done.CompletedAt);
    }

    [Fact]
    public async Task Progression_EventWithoutNewXp_StillCompletesAChallenge()
    {
        // Quiz refeito (sem XP de quiz) com 90%: cumpre "Acerte 80%".
        _progressionRepository.TotalXp[UserId] = 100;
        _challenges.Activity = new DailyActivity(0, 0, 1, 90);

        var reward = await _progression.GrantAllAsync(UserId, []);

        Assert.Equal("Acerte 80% de um quiz", Assert.Single(reward!.ChallengesCompleted).Title);
        Assert.Equal((40, 140), (reward.XpGained, reward.TotalXp));
    }

    [Fact]
    public async Task Progression_NothingMet_ReturnsNoChallenges()
    {
        var reward = await _progression.GrantAsync(UserId, ExperienceType.NationDiscovered, 4, "Descobriu a Nação do Fogo", new object());

        Assert.Empty(reward!.ChallengesCompleted);
        Assert.Equal(10, reward.XpGained);
    }

    [Fact]
    public async Task CompletingChallenges_CountsForElementMaster()
    {
        var achievements = new FakeAchievementRepository(_progressionRepository);
        achievements.Catalog.Add(new Achievement
        {
            Id = 6, Code = "ELEMENT_MASTER", Name = "Mestre dos elementos", Description = "-", Icon = "sparkles",
            RequirementType = AchievementRequirementType.ChallengesCompleted, RequirementValue = 2, ExperienceReward = 100
        });
        var progression = TestProgression.Create(_progressionRepository, achievements, _time, _challenges);
        _challenges.Activity = new DailyActivity(3, 1, 0, 0);

        var reward = await progression.GrantAsync(UserId, ExperienceType.NationDiscovered, 4, "Descobriu a Nação do Fogo", new object());

        // Dois desafios concluídos e, no mesmo evento, a conquista que conta desafios.
        Assert.Equal(2, reward!.ChallengesCompleted.Count);
        Assert.Equal("Mestre dos elementos", Assert.Single(reward.AchievementsUnlocked).Name);
        Assert.Equal(10 + 30 + 30 + 100, reward.XpGained);
    }

    [Fact]
    public async Task NextUtcDay_HasNewChallengesAndOnlyCountsThatDay()
    {
        _challenges.Activity = new DailyActivity(3, 0, 0, 0);
        await _progression.GrantAllAsync(UserId, []);
        var yesterday = await _service.GetTodayAsync(UserId);

        // 10 minutos depois já é outro dia em UTC; a atividade do novo dia começa zerada.
        _time.Advance(TimeSpan.FromMinutes(10));
        _challenges.Activity = new DailyActivity(0, 0, 0, 0);
        var today = await _service.GetTodayAsync(UserId);

        Assert.Equal(new DateOnly(2026, 10, 28), today.Date);
        Assert.Equal(2, _challenges.AddCalls);
        Assert.Empty(today.Challenges.Select(c => c.Id).Intersect(yesterday.Challenges.Select(c => c.Id)));
        Assert.All(today.Challenges, c => Assert.False(c.IsCompleted));
        Assert.Contains(yesterday.Challenges, c => c.IsCompleted);
    }

    [Fact]
    public async Task History_ListsCompletedChallengesNewestFirst()
    {
        _challenges.Activity = new DailyActivity(3, 1, 0, 0);
        await _progression.GrantAllAsync(UserId, []);

        var history = await _service.GetHistoryAsync(UserId, new PageQueryParameters());

        Assert.Equal(2, history.TotalItems);
        Assert.All(history.Items, c => Assert.Equal(new DateOnly(2026, 10, 27), c.Date));
    }
}
