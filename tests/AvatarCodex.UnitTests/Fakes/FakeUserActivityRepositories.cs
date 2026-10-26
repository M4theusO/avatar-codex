using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;

namespace AvatarCodex.UnitTests.Fakes;

/// <summary>Favoritos e descobertas em memória.</summary>
public sealed class FakeUserCollectionRepository : IUserCollectionRepository
{
    public HashSet<(Guid UserId, int CharacterId)> Favorites { get; } = [];
    public HashSet<(Guid UserId, int CharacterId)> Discovered { get; } = [];

    public Task<UserMarks> GetMarksAsync(Guid userId, IReadOnlyCollection<int> characterIds, CancellationToken cancellationToken) =>
        Task.FromResult(new UserMarks(
            characterIds.Where(id => Favorites.Contains((userId, id))).ToHashSet(),
            characterIds.Where(id => Discovered.Contains((userId, id))).ToHashSet()));

    public Task AddFavoriteAsync(Guid userId, int characterId, DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        Favorites.Add((userId, characterId));
        return Task.CompletedTask;
    }

    public Task RemoveFavoriteAsync(Guid userId, int characterId, CancellationToken cancellationToken)
    {
        Favorites.Remove((userId, characterId));
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<Character> Items, int TotalItems)> GetFavoritesAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken) =>
        Task.FromResult<(IReadOnlyList<Character>, int)>(([], Favorites.Count(f => f.UserId == userId)));

    public Task<(IReadOnlyList<DiscoveryEntry> Items, int TotalItems)> GetDiscoveriesAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken) =>
        Task.FromResult<(IReadOnlyList<DiscoveryEntry>, int)>(([], 0));

    public Task<UserStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(new UserStatisticsDto(0, 0, 0, 0, Favorites.Count(f => f.UserId == userId)));
}

/// <summary>
/// Simula a transação de progresso: a unique (UserId, Type, SourceId) vira um HashSet,
/// e o total de XP fica num dicionário por usuário.
/// </summary>
public sealed class FakeProgressionRepository : IProgressionRepository
{
    public List<object> Records { get; } = [];
    public List<ExperienceTransaction> Transactions { get; } = [];
    public Dictionary<Guid, int> TotalXp { get; } = [];

    public Task<int?> SaveWithExperienceAsync(object activityRecord, ExperienceTransaction transaction, CancellationToken cancellationToken)
    {
        var alreadyGranted = Transactions.Any(t =>
            t.UserId == transaction.UserId && t.Type == transaction.Type && t.SourceId == transaction.SourceId);
        if (alreadyGranted)
            return Task.FromResult<int?>(null);

        Records.Add(activityRecord);
        Transactions.Add(transaction);
        TotalXp[transaction.UserId] = TotalXp.GetValueOrDefault(transaction.UserId) + transaction.Amount;
        return Task.FromResult<int?>(TotalXp[transaction.UserId]);
    }
}
