using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/nations")]
[Produces("application/json")]
public class NationsController(NationService nations) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NationDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NationDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await nations.GetAllAsync(cancellationToken));

    /// <summary>Detalhe da nação com seus personagens.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<NationDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NationDetailsDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await nations.GetByIdAsync(id, cancellationToken));
}
