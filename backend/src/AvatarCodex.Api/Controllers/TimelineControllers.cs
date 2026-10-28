using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/timeline")]
[Produces("application/json")]
public class TimelineController(TimelineService timeline) : ControllerBase
{
    /// <summary>
    /// Eventos em ordem cronológica, agrupados por era, com os personagens relacionados.
    /// Filtros opcionais: eraId e characterId.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TimelineEraDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TimelineEraDto>>> Get(
        [FromQuery] TimelineQueryParameters query, CancellationToken cancellationToken) =>
        Ok(await timeline.GetTimelineAsync(query, cancellationToken));
}

[ApiController]
[Route("api/eras")]
[Produces("application/json")]
public class ErasController(TimelineService timeline) : ControllerBase
{
    /// <summary>Eras em ordem cronológica, com a quantidade de eventos (para o filtro da linha do tempo).</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<EraDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EraDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await timeline.GetErasAsync(cancellationToken));
}
