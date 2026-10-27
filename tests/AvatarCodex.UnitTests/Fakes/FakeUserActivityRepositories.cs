using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
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

    public Task<IReadOnlyList<Character>> GetUndiscoveredCharactersAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Character>>([]);

    public Task<(IReadOnlyList<DiscoveryEntry> Items, int TotalItems)> GetDiscoveriesAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken) =>
        Task.FromResult<(IReadOnlyList<DiscoveryEntry>, int)>(([], 0));

    public Task<UserStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(new UserStatisticsDto(0, 0, 0, 0, Favorites.Count(f => f.UserId == userId), 0, 0, 0, 0, 0, null));

    public List<ExperienceTransaction> Experience { get; } = [];

    public Task<(IReadOnlyList<ExperienceTransaction> Items, int TotalItems)> GetExperienceAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var mine = Experience.Where(t => t.UserId == userId).OrderByDescending(t => t.CreatedAt).ToList();
        return Task.FromResult<(IReadOnlyList<ExperienceTransaction>, int)>(
            ([.. mine.Skip((page - 1) * pageSize).Take(pageSize)], mine.Count));
    }
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

    public Task<int?> SaveWithExperienceAsync(object? activityRecord, ExperienceTransaction transaction, CancellationToken cancellationToken)
    {
        var alreadyGranted = Transactions.Any(t =>
            t.UserId == transaction.UserId && t.Type == transaction.Type && t.SourceId == transaction.SourceId);
        if (alreadyGranted)
            return Task.FromResult<int?>(null);

        if (activityRecord is not null)
            Records.Add(activityRecord);
        Transactions.Add(transaction);
        TotalXp[transaction.UserId] = TotalXp.GetValueOrDefault(transaction.UserId) + transaction.Amount;
        return Task.FromResult<int?>(TotalXp[transaction.UserId]);
    }
}

/// <summary>
/// Catálogo configurável. Desbloqueios e descobertas são lidos dos registros gravados no
/// FakeProgressionRepository, como o banco faria.
/// </summary>
public sealed class FakeAchievementRepository(FakeProgressionRepository progression) : IAchievementRepository
{
    public List<Achievement> Catalog { get; } = [];
    public int TotalNations { get; set; } = 5;
    public int CorrectAnswers { get; set; }
    public int QuizzesCompleted { get; set; }
    public int PerfectQuizzes { get; set; }

    public Task<IReadOnlyList<Achievement>> GetCatalogAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Achievement>>(Catalog);

    public Task<IReadOnlyDictionary<int, DateTimeOffset>> GetUnlockedAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<int, DateTimeOffset>>(progression.Records
            .OfType<UserAchievement>()
            .Where(u => u.UserId == userId)
            .ToDictionary(u => u.AchievementId, u => u.UnlockedAt));

    public Task<ProgressMetrics> GetMetricsAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(new ProgressMetrics(
            progression.TotalXp.GetValueOrDefault(userId),
            progression.Records.OfType<UserCharacterDiscovery>().Count(d => d.UserId == userId),
            progression.Records.OfType<UserNationDiscovery>().Count(d => d.UserId == userId),
            TotalNations,
            QuizzesCompleted: QuizzesCompleted,
            PerfectQuizzes: PerfectQuizzes,
            CorrectAnswers: CorrectAnswers,
            ChallengesCompleted: progression.Records.OfType<UserDailyChallenge>().Count(u => u.UserId == userId)));
}

/// <summary>
/// Modelos e desafios do dia em memória. Conclusões são lidas dos registros do FakeProgressionRepository;
/// a atividade do dia é configurada pelo teste.
/// </summary>
public sealed class FakeChallengeRepository(FakeProgressionRepository progression) : IChallengeRepository
{
    public List<ChallengeTemplate> Templates { get; } = [];
    public List<DailyChallenge> Daily { get; } = [];
    public DailyActivity Activity { get; set; } = new(0, 0, 0, 0);
    public int AddCalls { get; private set; }

    public Task<IReadOnlyList<ChallengeTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ChallengeTemplate>>([.. Templates.Where(t => t.IsActive).OrderBy(t => t.Id)]);

    public Task<IReadOnlyList<DailyChallenge>> GetForDateAsync(DateOnly date, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DailyChallenge>>([.. Daily.Where(d => d.Date == date).OrderBy(d => d.Id)]);

    public Task AddForDateAsync(DateOnly date, IReadOnlyList<int> templateIds, CancellationToken cancellationToken)
    {
        AddCalls++;
        foreach (var id in templateIds.Where(id => !Daily.Any(d => d.Date == date && d.ChallengeTemplateId == id)))
            Daily.Add(new DailyChallenge
            {
                Id = Daily.Count + 1,
                ChallengeTemplateId = id,
                Template = Templates.Single(t => t.Id == id),
                Date = date
            });
        return Task.CompletedTask;
    }

    public Task<IReadOnlyDictionary<int, DateTimeOffset>> GetCompletedAsync(
        Guid userId, IReadOnlyCollection<int> dailyChallengeIds, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<int, DateTimeOffset>>(progression.Records
            .OfType<UserDailyChallenge>()
            .Where(u => u.UserId == userId && dailyChallengeIds.Contains(u.DailyChallengeId))
            .ToDictionary(u => u.DailyChallengeId, u => u.CompletedAt));

    public Task<DailyActivity> GetActivityAsync(Guid userId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        Task.FromResult(Activity);

    public Task<(IReadOnlyList<UserDailyChallenge> Items, int TotalItems)> GetHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var mine = progression.Records.OfType<UserDailyChallenge>()
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.CompletedAt)
            .Select(u =>
            {
                u.DailyChallenge = Daily.Single(d => d.Id == u.DailyChallengeId);
                return u;
            })
            .ToList();
        return Task.FromResult<(IReadOnlyList<UserDailyChallenge>, int)>(
            ([.. mine.Skip((page - 1) * pageSize).Take(pageSize)], mine.Count));
    }
}

public static class TestProgression
{
    /// <summary>ProgressionService com fakes. Sem modelos de desafio, nenhum desafio é gerado nem concluído.</summary>
    public static ProgressionService Create(
        FakeProgressionRepository repository,
        FakeAchievementRepository achievements,
        TimeProvider timeProvider,
        FakeChallengeRepository? challenges = null) =>
        new(
            repository,
            achievements,
            new DailyChallengeService(challenges ?? new FakeChallengeRepository(repository), timeProvider, new PageQueryParametersValidator()),
            timeProvider);
}

/// <summary>Relógio controlado pelo teste (ex.: simular a virada do dia).</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    private DateTimeOffset _now = now;
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan by) => _now += by;
}
