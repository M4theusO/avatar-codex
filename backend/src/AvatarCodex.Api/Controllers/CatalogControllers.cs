using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
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
public class AbilitiesController(CatalogService catalog) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AbilityDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AbilityDto>>> GetAll(
        [FromQuery] int? elementId, CancellationToken cancellationToken) =>
        Ok(await catalog.GetAbilitiesAsync(elementId, cancellationToken));
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
