using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.UnitTests.Fakes;

namespace AvatarCodex.UnitTests.Progress;

public class AchievementTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static readonly Achievement FirstStep = New(1, "Primeiro passo", AchievementRequirementType.AccountCreated, 0, 10);
    private static readonly Achievement Curious = New(2, "Curioso", AchievementRequirementType.CharactersDiscovered, 10, 50);
    private static readonly Achievement Historian = New(3, "Historiador", AchievementRequirementType.AllNationsDiscovered, 0, 50);
    private static readonly Achievement Level2 = New(4, "Aprendiz", AchievementRequirementType.LevelReached, 2, 20);

    private readonly FakeProgressionRepository _progressionRepository = new();
    private readonly FakeAchievementRepository _achievements;
    private readonly ProgressionService _progression;

    public AchievementTests()
    {
        _achievements = new FakeAchievementRepository(_progressionRepository);
        _progression = TestProgression.Create(_progressionRepository, _achievements, TimeProvider.System);
    }

    private static Achievement New(int id, string name, AchievementRequirementType type, int value, int xp) => new()
    {
        Id = id, Code = name.ToUpperInvariant(), Name = name, Description = name, Icon = "award",
        RequirementType = type, RequirementValue = value, ExperienceReward = xp
    };

    private static ProgressMetrics Metrics(int xp = 0, int characters = 0, int nations = 0, int totalNations = 5) =>
        new(xp, characters, nations, totalNations);

    private Task<Application.DTOs.RewardResult?> DiscoverCharacterAsync(int characterId) =>
        _progression.GrantAsync(UserId, ExperienceType.CharacterDiscovered, characterId, $"Descobriu {characterId}",
            new UserCharacterDiscovery { UserId = UserId, CharacterId = characterId });

    private async Task DiscoverCharactersAsync(int count)
    {
        for (var id = 1; id <= count; id++)
            await DiscoverCharacterAsync(id);
    }

    [Theory]
    [InlineData(9, false)]
    [InlineData(10, true)]
    [InlineData(12, true)]
    public void Rules_CharactersDiscovered_UnlocksAtTheTarget(int discovered, bool expected)
    {
        Assert.Equal(expected, AchievementRules.IsMet(Curious, Metrics(characters: discovered)));
        Assert.Equal(Math.Min(discovered, 10), AchievementRules.Progress(Curious, Metrics(characters: discovered)));
    }

    [Fact]
    public void Rules_AllNations_FollowsTheNumberOfNationsInTheContent()
    {
        Assert.Equal(5, AchievementRules.Target(Historian, Metrics(totalNations: 5)));
        Assert.False(AchievementRules.IsMet(Historian, Metrics(nations: 4, totalNations: 5)));
        Assert.True(AchievementRules.IsMet(Historian, Metrics(nations: 5, totalNations: 5)));
        Assert.False(AchievementRules.IsMet(Historian, Metrics(nations: 0, totalNations: 0)));
    }

    [Fact]
    public void Rules_AccountCreatedAndLevel()
    {
        Assert.True(AchievementRules.IsMet(FirstStep, Metrics()));
        Assert.False(AchievementRules.IsMet(Level2, Metrics(xp: 99)));
        Assert.True(AchievementRules.IsMet(Level2, Metrics(xp: 100)));
    }

    [Theory]
    [InlineData(AchievementRequirementType.QuizzesCompleted, 3)]
    [InlineData(AchievementRequirementType.PerfectQuizzes, 4)]
    [InlineData(AchievementRequirementType.CorrectAnswers, 5)]
    [InlineData(AchievementRequirementType.ChallengesCompleted, 6)]
    [InlineData(AchievementRequirementType.CharactersDiscovered, 7)]
    [InlineData(AchievementRequirementType.NationsDiscovered, 2)]
    [InlineData(AchievementRequirementType.LevelReached, 2)]
    public void Rules_EachRequirementTypeReadsItsOwnMetric(AchievementRequirementType type, int expected)
    {
        // Valores diferentes em cada métrica: trocar uma pela outra faria o teste falhar.
        var metrics = new ProgressMetrics(
            ExperiencePoints: 150, CharactersDiscovered: 7, NationsDiscovered: 2, TotalNations: 5,
            QuizzesCompleted: 3, PerfectQuizzes: 4, CorrectAnswers: 5, ChallengesCompleted: 6);

        Assert.Equal(expected, AchievementRules.Current(New(9, "Teste", type, 100, 10), metrics));
    }

    [Fact]
    public async Task Grant_TenthDiscovery_UnlocksCuriousWithItsXpInTheSameReward()
    {
        _achievements.Catalog.Add(Curious);
        await DiscoverCharactersAsync(9);

        var reward = await DiscoverCharacterAsync(10);

        Assert.NotNull(reward);
        var unlocked = Assert.Single(reward.AchievementsUnlocked);
        Assert.Equal(("Curioso", 50), (unlocked.Name, unlocked.ExperienceReward));
        Assert.Equal(60, reward.XpGained);
        Assert.Equal(150, reward.TotalXp);
        Assert.True(reward.LeveledUp);

        var transaction = _progressionRepository.Transactions.Last();
        Assert.Equal((ExperienceType.AchievementUnlocked, Curious.Id, 50), (transaction.Type, transaction.SourceId, transaction.Amount));
        Assert.Equal("Conquista: Curioso", transaction.Description);
    }

    [Fact]
    public async Task Grant_AchievementIsUnlockedOnlyOnce()
    {
        _achievements.Catalog.Add(Curious);
        await DiscoverCharactersAsync(10);

        var reward = await DiscoverCharacterAsync(11);

        Assert.Empty(reward!.AchievementsUnlocked);
        Assert.Equal(10, reward.XpGained);
        Assert.Single(_progressionRepository.Records.OfType<UserAchievement>());
    }

    [Fact]
    public async Task Grant_AchievementXpDoesNotTriggerANewEvaluation()
    {
        // RN-08: a 10ª descoberta leva de 40 a 50 XP (nível 1); só o XP do Curioso leva a 100 (nível 2).
        // "Aprendiz" (nível 2) não pode sair dessa mesma avaliação.
        _achievements.Catalog.AddRange([Curious, Level2]);
        await DiscoverCharactersAsync(9);
        _progressionRepository.TotalXp[UserId] = 40;

        var reward = await DiscoverCharacterAsync(10);

        Assert.Equal(["Curioso"], reward!.AchievementsUnlocked.Select(a => a.Name));
        Assert.Equal(100, reward.TotalXp);

        // No próximo evento a meta de nível é avaliada normalmente.
        var next = await DiscoverCharacterAsync(11);
        Assert.Equal(["Aprendiz"], next!.AchievementsUnlocked.Select(a => a.Name));
    }

    [Fact]
    public async Task Grant_PendingAchievementsFromBeforeArePickedUpByTheNextEvent()
    {
        // Usuário criado antes das conquistas existirem: o "Primeiro passo" sai na próxima atividade.
        _achievements.Catalog.Add(FirstStep);

        var reward = await DiscoverCharacterAsync(1);

        Assert.Equal("Primeiro passo", Assert.Single(reward!.AchievementsUnlocked).Name);
        Assert.Equal(20, reward.XpGained);
    }

    [Fact]
    public async Task Grant_RepeatedEvent_DoesNotEvaluateAchievements()
    {
        await DiscoverCharacterAsync(1);
        _achievements.Catalog.Add(FirstStep);

        Assert.Null(await DiscoverCharacterAsync(1));
        Assert.Empty(_progressionRepository.Records.OfType<UserAchievement>());
    }

    [Fact]
    public async Task Evaluate_NothingToUnlock_ReturnsNull()
    {
        _achievements.Catalog.Add(Curious);

        Assert.Null(await _progression.EvaluateAchievementsAsync(UserId));
    }

    [Fact]
    public async Task GetForUser_ShowsUnlockedWithDateAndLockedWithProgress()
    {
        _achievements.Catalog.AddRange([FirstStep, Curious, Historian]);
        await _progression.EvaluateAchievementsAsync(UserId);
        await DiscoverCharactersAsync(3);
        var service = new AchievementService(_achievements);

        var items = await service.GetForUserAsync(UserId);

        Assert.Equal(["Primeiro passo", "Curioso", "Historiador"], items.Select(i => i.Name));
        Assert.True(items[0].IsUnlocked);
        Assert.NotNull(items[0].UnlockedAt);
        Assert.Equal((1, 1), (items[0].Progress, items[0].Target));
        Assert.False(items[1].IsUnlocked);
        Assert.Null(items[1].UnlockedAt);
        Assert.Equal((3, 10), (items[1].Progress, items[1].Target));
        Assert.Equal((0, 5), (items[2].Progress, items[2].Target));
    }
}
