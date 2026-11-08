using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

/// <summary>Fotos de perfil, públicas pelo id da URL (que não é o id do usuário e muda a cada envio).</summary>
[ApiController]
[Route("api/avatars")]
public class AvatarsController(AvatarService avatars) : ControllerBase
{
    /// <summary>A imagem. Cache longo: um envio novo gera outra URL.</summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid publicId, CancellationToken cancellationToken)
    {
        var avatar = await avatars.GetAsync(publicId, cancellationToken);
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(avatar.Data, avatar.ContentType);
    }
}
