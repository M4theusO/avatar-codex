namespace AvatarCodex.Application.DTOs;

/// <summary>Tudo o que a página inicial do usuário mostra, numa única chamada (RF-DASH-01).</summary>
public record DashboardDto(
    UserDto User,
    UserStatisticsDto Statistics,
    DailyChallengesDto DailyChallenges,
    /// <summary>Desbloqueadas, mais recentes primeiro.</summary>
    IReadOnlyList<UserAchievementDto> RecentAchievements,
    /// <summary>A bloqueada mais perto da meta; null quando todas foram desbloqueadas.</summary>
    UserAchievementDto? NextAchievement,
    IReadOnlyList<DiscoveryDto> RecentDiscoveries,
    /// <summary>Personagens ainda não descobertos, com rotação diária.</summary>
    IReadOnlyList<CharacterListItemDto> Recommendations,
    /// <summary>Primeiro quiz não concluído, ou o mais perto do gabarito; null quando todos foram gabaritados.</summary>
    QuizListItemDto? FeaturedQuiz,
    IReadOnlyList<CharacterListItemDto> Favorites);
