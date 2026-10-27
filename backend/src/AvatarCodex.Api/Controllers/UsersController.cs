using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

/// <summary>Dados do usuário autenticado. O id vem sempre do token, nunca do cliente.</summary>
[ApiController]
[Authorize]
[Route("api/users/me")]
[Produces("application/json")]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
public class UsersController(
    UserService users,
    FavoriteService favorites,
    DiscoveryService discoveries,
    AchievementService achievements,
    QuizService quizzes,
    DashboardService dashboard) : ControllerBase
{
    /// <summary>
    /// Página inicial do usuário numa chamada: progresso, estatísticas, conquistas, descobertas,
    /// recomendações, quiz em destaque e favoritos.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType<DashboardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await dashboard.GetAsync(User.GetUserId(), cancellationToken));

    /// <summary>Descobertas, favoritos, conquistas, quizzes e taxa de acerto.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType<UserStatisticsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserStatisticsDto>> GetStatistics(CancellationToken cancellationToken) =>
        Ok(await users.GetStatisticsAsync(User.GetUserId(), cancellationToken));

    /// <summary>Favoritos, mais recentes primeiro.</summary>
    [HttpGet("favorites")]
    [ProducesResponseType<PagedResult<CharacterListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CharacterListItemDto>>> GetFavorites(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await favorites.GetPagedAsync(User.GetUserId(), query, cancellationToken));

    /// <summary>Histórico de XP (cada movimentação), mais recente primeiro.</summary>
    [HttpGet("experience")]
    [ProducesResponseType<PagedResult<ExperienceTransactionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ExperienceTransactionDto>>> GetExperience(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await users.GetExperienceHistoryAsync(User.GetUserId(), query, cancellationToken));

    /// <summary>Tentativas de quiz (todas, inclusive as sem XP), mais recentes primeiro.</summary>
    [HttpGet("quiz-history")]
    [ProducesResponseType<PagedResult<QuizAttemptDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<QuizAttemptDto>>> GetQuizHistory(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await quizzes.GetHistoryAsync(User.GetUserId(), query, cancellationToken));

    /// <summary>Catálogo completo: desbloqueadas com data, bloqueadas com progresso.</summary>
    [HttpGet("achievements")]
    [ProducesResponseType<IReadOnlyList<UserAchievementDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserAchievementDto>>> GetAchievements(CancellationToken cancellationToken) =>
        Ok(await achievements.GetForUserAsync(User.GetUserId(), cancellationToken));

    /// <summary>Descobertas de personagens e nações, mais recentes primeiro.</summary>
    [HttpGet("discoveries")]
    [ProducesResponseType<PagedResult<DiscoveryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<DiscoveryDto>>> GetDiscoveries(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await discoveries.GetHistoryAsync(User.GetUserId(), query, cancellationToken));
}
