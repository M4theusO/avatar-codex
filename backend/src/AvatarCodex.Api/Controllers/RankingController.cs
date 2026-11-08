using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

/// <summary>Ranking por XP (RF-RANK-01). Só para quem está logado: mostra nomes de usuário.</summary>
[ApiController]
[Authorize]
[Route("api/ranking")]
[Produces("application/json")]
public class RankingController(RankingService ranking) : ControllerBase
{
    /// <summary>
    /// period = all (XP total) ou month (XP do mês, UTC); elementId opcional (elemento do teste).
    /// Empatados dividem a posição. Inclui a posição de quem consulta (currentUser).
    /// </summary>
    [HttpGet]
    [ProducesResponseType<RankingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RankingDto>> Get(
        [FromQuery] RankingQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await ranking.GetAsync(User.GetUserId(), query, cancellationToken));
}
