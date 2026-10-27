using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

/// <summary>
/// Desafios diários. Não há endpoint para concluir (D9): a conclusão é automática no evento que cumpre
/// a meta, e a recompensa volta no reward dele.
/// </summary>
[ApiController]
[Authorize]
[Route("api/challenges")]
[Produces("application/json")]
[ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
public class ChallengesController(DailyChallengeService challenges) : ControllerBase
{
    /// <summary>Desafios de hoje (UTC) com o progresso do usuário; gera os do dia se ainda não existirem.</summary>
    [HttpGet("daily")]
    [ProducesResponseType<DailyChallengesDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DailyChallengesDto>> GetDaily(CancellationToken cancellationToken) =>
        Ok(await challenges.GetTodayAsync(User.GetUserId(), cancellationToken));
}
