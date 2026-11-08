using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/elements")]
[Produces("application/json")]
public class ElementsController(CatalogService catalog) : ControllerBase
{
    /// <summary>Elementos com seus subtipos de dobra.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ElementDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ElementDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await catalog.GetElementsAsync(cancellationToken));
}

[ApiController]
[Route("api/abilities")]
[Produces("application/json")]
public class AbilitiesController(CatalogService catalog, DiscoveryService discoveries) : ControllerBase
{
    /// <summary>Habilidades (ordem dos elementos, depois nome) com a quantidade de personagens.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AbilityListItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AbilityListItemDto>>> GetAll(
        [FromQuery] int? elementId, CancellationToken cancellationToken) =>
        Ok(await catalog.GetAbilitiesAsync(elementId, cancellationToken));

    /// <summary>Detalhe da habilidade com quem a possui. Com token, preenche isFavorite/isDiscovered.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<AbilityDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AbilityDetailsDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await catalog.GetAbilityByIdAsync(id, User.GetUserIdOrNull(), cancellationToken));

    /// <summary>Registra a descoberta da habilidade (idempotente): +10 XP só na primeira vez.</summary>
    [Authorize]
    [HttpPost("{id:int}/discovery")]
    [ProducesResponseType<DiscoveryResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiscoveryResultDto>> Discover(int id, CancellationToken cancellationToken) =>
        Ok(await discoveries.DiscoverAbilityAsync(User.GetUserId(), id, cancellationToken));
}

[ApiController]
[Route("api/series")]
[Produces("application/json")]
public class SeriesController(CatalogService catalog) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SeriesDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SeriesDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await catalog.GetSeriesAsync(cancellationToken));
}
