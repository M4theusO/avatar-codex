using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/nations")]
[Produces("application/json")]
public class NationsController(NationService nations, DiscoveryService discoveries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NationDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NationDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await nations.GetAllAsync(cancellationToken));

    /// <summary>Detalhe da nação com seus personagens. Com token, preenche isFavorite/isDiscovered.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<NationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NationDetailsDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await nations.GetByIdAsync(id, User.GetUserIdOrNull(), cancellationToken));

    /// <summary>Registra a descoberta da nação (idempotente): +10 XP só na primeira vez (DA-01).</summary>
    [Authorize]
    [HttpPost("{id:int}/discovery")]
    [ProducesResponseType<DiscoveryResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscoveryResultDto>> Discover(int id, CancellationToken cancellationToken) =>
        Ok(await discoveries.DiscoverNationAsync(User.GetUserId(), id, cancellationToken));
}
