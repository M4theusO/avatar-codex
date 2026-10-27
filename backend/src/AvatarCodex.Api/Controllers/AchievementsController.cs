using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/achievements")]
[Produces("application/json")]
public class AchievementsController(AchievementService achievements) : ControllerBase
{
    /// <summary>Catálogo público de conquistas. O progresso do usuário fica em /api/users/me/achievements.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AchievementDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AchievementDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await achievements.GetCatalogAsync(cancellationToken));
}
