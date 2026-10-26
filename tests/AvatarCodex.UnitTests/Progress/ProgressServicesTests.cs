using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using AvatarCodex.UnitTests.Fakes;

namespace AvatarCodex.UnitTests.Progress;

public class ProgressServicesTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly FakeProgressionRepository _progressionRepository = new();
    private readonly FakeUserCollectionRepository _collection = new();
    private readonly FakeContentRepository _content = new();
    private readonly ProgressionService _progression;
    private readonly DiscoveryService _discoveries;
    private readonly FavoriteService _favorites;

    public ProgressServicesTests()
    {
        _progression = new ProgressionService(_progressionRepository, TimeProvider.System);
        _discoveries = new DiscoveryService(_content, _content, _collection, _progression, TimeProvider.System, new PageQueryParametersValidator());
        _favorites = new FavoriteService(_content, _collection, TimeProvider.System, new PageQueryParametersValidator());
    }

    [Fact]
    public async Task Grant_UsesOfficialXpTableAndRecordsTransaction()
    {
        var reward = await _progression.GrantAsync(UserId, ExperienceType.CharacterDiscovered, 13, "Descobriu Zuko", new object());

        Assert.NotNull(reward);
        Assert.Equal((10, 10, 1, false), (reward.XpGained, reward.TotalXp, reward.Level, reward.LeveledUp));
        Assert.Empty(reward.AchievementsUnlocked);
        var transaction = Assert.Single(_progressionRepository.Transactions);
        Assert.Equal(10, transaction.Amount);
        Assert.Equal("Descobriu Zuko", transaction.Description);
    }

    [Fact]
    public async Task Grant_SameRewardTwice_ReturnsNullWithoutXp()
    {
        await _progression.GrantAsync(UserId, ExperienceType.NationDiscovered, 4, "Descobriu a Nação do Fogo", new object());

        var second = await _progression.GrantAsync(UserId, ExperienceType.NationDiscovered, 4, "Descobriu a Nação do Fogo", new object());

        Assert.Null(second);
        Assert.Equal(10, _progressionRepository.TotalXp[UserId]);
    }

    [Fact]
    public async Task Grant_CrossingLevelThreshold_ReportsLevelUp()
    {
        _progressionRepository.TotalXp[UserId] = 95;

        var reward = await _progression.GrantAsync(UserId, ExperienceType.CharacterDiscovered, 1, "Descobriu Aang", new object());

        Assert.Equal(105, reward!.TotalXp);
        Assert.Equal(2, reward.Level);
        Assert.True(reward.LeveledUp);
    }

    [Theory]
    [InlineData(ExperienceType.ChallengeCompleted)]
    [InlineData(ExperienceType.AchievementUnlocked)]
    public void ExperienceRewards_VariableTypes_HaveNoFixedValue(ExperienceType type)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ExperienceRewards.For(type));
    }

    [Fact]
    public async Task DiscoverCharacter_FirstTimeIsNewThenNot()
    {
        var first = await _discoveries.DiscoverCharacterAsync(UserId, FakeContentRepository.ZukoId);
        var second = await _discoveries.DiscoverCharacterAsync(UserId, FakeContentRepository.ZukoId);

        Assert.True(first.IsNew);
        Assert.Equal(10, first.Reward!.XpGained);
        Assert.Equal(new DiscoveryResultDto(false, null), second);

        var record = Assert.IsType<UserCharacterDiscovery>(Assert.Single(_progressionRepository.Records));
        Assert.Equal((UserId, FakeContentRepository.ZukoId), (record.UserId, record.CharacterId));
        Assert.Equal("Descobriu Zuko", _progressionRepository.Transactions.Single().Description);
    }

    [Fact]
    public async Task DiscoverNation_GrantsTenXp()
    {
        var result = await _discoveries.DiscoverNationAsync(UserId, FakeContentRepository.FireNationId);

        Assert.True(result.IsNew);
        Assert.Equal(ExperienceType.NationDiscovered, _progressionRepository.Transactions.Single().Type);
        Assert.IsType<UserNationDiscovery>(_progressionRepository.Records.Single());
    }

    [Fact]
    public async Task Discover_UnknownContent_ThrowsNotFoundWithoutXp()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _discoveries.DiscoverCharacterAsync(UserId, 999));
        await Assert.ThrowsAsync<NotFoundException>(() => _discoveries.DiscoverNationAsync(UserId, 999));

        Assert.Empty(_progressionRepository.Transactions);
    }

    [Fact]
    public async Task Favorites_AddIsIdempotentAndRemoveWorks()
    {
        await _favorites.AddAsync(UserId, FakeContentRepository.ZukoId);
        await _favorites.AddAsync(UserId, FakeContentRepository.ZukoId);
        Assert.Single(_collection.Favorites);

        await _favorites.RemoveAsync(UserId, FakeContentRepository.ZukoId);
        Assert.Empty(_collection.Favorites);
    }

    [Fact]
    public async Task Favorites_UnknownCharacter_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _favorites.AddAsync(UserId, 999));
        await Assert.ThrowsAsync<NotFoundException>(() => _favorites.RemoveAsync(UserId, 999));
    }

    [Fact]
    public async Task Favorites_InvalidPage_ThrowsValidation()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _favorites.GetPagedAsync(UserId, new PageQueryParameters { PageSize = 51 }));
    }

    private sealed class FakeContentRepository : ICharacterRepository, INationRepository
    {
        public const int ZukoId = 13;
        public const int FireNationId = 4;

        Task<string?> ICharacterRepository.GetNameAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(id == ZukoId ? "Zuko" : null);

        Task<string?> INationRepository.GetNameAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(id == FireNationId ? "Nação do Fogo" : null);

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) => Task.FromResult(id == ZukoId);

        public Task<(IReadOnlyList<Character> Items, int TotalItems)> GetPagedAsync(CharacterFilter filter, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        Task<Character?> ICharacterRepository.GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<Ability>> GetAbilitiesAsync(int characterId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<(Nation Nation, int CharacterCount)>> GetAllAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        Task<Nation?> INationRepository.GetDetailsAsync(int id, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
