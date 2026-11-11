using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

/// <summary>Perfis públicos (RF-SOCIAL-01). Só para quem está logado, como o ranking.</summary>
[ApiController]
[Authorize]
[Route("api/users")]
[Produces("application/json")]
public class ProfilesController(PublicProfileService profiles, FollowService follows) : ControllerBase
{
    /// <summary>
    /// Perfil de um usuário pelo nome (sem diferenciar maiúsculas/minúsculas): nível, XP, elemento,
    /// estatísticas e conquistas desbloqueadas. Inexistente ou privado (de outra pessoa) → 404.
    /// </summary>
    [HttpGet("{username}")]
    [ProducesResponseType<PublicProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProfileDto>> Get(string username, CancellationToken cancellationToken) =>
        Ok(await profiles.GetAsync(User.GetUserId(), username, cancellationToken));

    /// <summary>
    /// Segue o usuário (sem XP). Idempotente. Inexistente ou privado → 404; a si mesmo ou a conta demo → 422.
    /// </summary>
    [HttpPut("{username}/follow")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Follow(string username, CancellationToken cancellationToken)
    {
        await follows.FollowAsync(User.GetUserId(), username, cancellationToken);
        return NoContent();
    }

    /// <summary>Deixa de seguir. Sempre 204 (idempotente; não revela se a conta existe).</summary>
    [HttpDelete("{username}/follow")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unfollow(string username, CancellationToken cancellationToken)
    {
        await follows.UnfollowAsync(User.GetUserId(), username, cancellationToken);
        return NoContent();
    }
}
