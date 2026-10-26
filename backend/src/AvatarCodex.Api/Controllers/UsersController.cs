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
    DiscoveryService discoveries) : ControllerBase
{
    /// <summary>Descobertas, total de personagens/nações e favoritos.</summary>
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

    /// <summary>Descobertas de personagens e nações, mais recentes primeiro.</summary>
    [HttpGet("discoveries")]
    [ProducesResponseType<PagedResult<DiscoveryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<DiscoveryDto>>> GetDiscoveries(
        [FromQuery] PageQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await discoveries.GetHistoryAsync(User.GetUserId(), query, cancellationToken));
}
