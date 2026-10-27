using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Exceptions;

namespace AvatarCodex.Application.Services;

/// <summary>Agregado da página inicial do usuário (RF-DASH-01): reaproveita os serviços de cada área.</summary>
public class DashboardService(
    IUserRepository users,
    IUserCollectionRepository collection,
    AchievementService achievements,
    QuizService quizzes,
    DailyChallengeService challenges,
    TimeProvider timeProvider)
{
    public const int RecentAchievementsCount = 3;
    public const int RecentDiscoveriesCount = 5;
    public const int RecommendationsCount = 4;
    public const int FavoritesCount = 4;

    // As consultas são sequenciais: todas usam o mesmo DbContext, que não aceita operações paralelas.
    public async Task<DashboardDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException(AuthService.InvalidSessionMessage);

        var statistics = await collection.GetStatisticsAsync(userId, cancellationToken);
        var dailyChallenges = await challenges.GetTodayAsync(userId, cancellationToken);

        var userAchievements = await achievements.GetForUserAsync(userId, cancellationToken);
        var recentAchievements = userAchievements
            .Where(a => a.IsUnlocked)
            .OrderByDescending(a => a.UnlockedAt)
            .Take(RecentAchievementsCount)
            .ToList();

        var (discoveries, _) = await collection.GetDiscoveriesAsync(userId, 1, RecentDiscoveriesCount, cancellationToken);

        var undiscovered = await collection.GetUndiscoveredCharactersAsync(userId, cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var recommended = DashboardRules.PickDaily(undiscovered, userId, today, RecommendationsCount);

        var (favorites, _) = await collection.GetFavoritesAsync(userId, 1, FavoritesCount, cancellationToken);
        var marks = await collection.GetMarksAsync(
            userId, [.. favorites.Concat(recommended).Select(c => c.Id).Distinct()], cancellationToken);

        var quizList = await quizzes.GetAllAsync(new QuizQueryParameters(), userId, cancellationToken);

        return new DashboardDto(
            user.ToDto(),
            statistics,
            dailyChallenges,
            recentAchievements,
            DashboardRules.NextAchievement(userAchievements),
            [.. discoveries.Select(d => d.ToDto())],
            [.. recommended.Select(c => marks.Apply(c.ToListItemDto()))],
            DashboardRules.FeaturedQuiz(quizList),
            [.. favorites.Select(c => marks.Apply(c.ToListItemDto()))]);
    }
}
