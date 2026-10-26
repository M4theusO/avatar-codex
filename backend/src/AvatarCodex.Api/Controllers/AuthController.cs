using AvatarCodex.Api.Extensions;
using AvatarCodex.Api.Middleware;
using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(AuthService auth) : ControllerBase
{
    public const string RefreshCookieName = "refresh_token";
    private const string RefreshCookiePath = "/api/auth";

    /// <summary>Cria a conta e já inicia a sessão.</summary>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.RegisterAsync(request, cancellationToken);
        SetRefreshCookie(result);
        return CreatedAtAction(nameof(Me), result.Response);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await auth.LoginAsync(request, cancellationToken);
        SetRefreshCookie(result);
        return Ok(result.Response);
    }

    /// <summary>Novo access token a partir do cookie de refresh, que é rotacionado.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        try
        {
            var result = await auth.RefreshAsync(Request.Cookies[RefreshCookieName], cancellationToken);
            SetRefreshCookie(result);
            return Ok(result.Response);
        }
        catch (UnauthorizedException ex)
        {
            // Responde aqui (e não pelo handler global, que limpa os headers) para apagar o cookie inválido.
            ClearRefreshCookie();
            return Unauthorized(new ErrorResponse(StatusCodes.Status401Unauthorized, ex.Message, []));
        }
    }

    /// <summary>Revoga a sessão atual. Funciona só com o cookie, mesmo com o access token expirado.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await auth.LogoutAsync(Request.Cookies[RefreshCookieName], cancellationToken);
        ClearRefreshCookie();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken) =>
        Ok(await auth.GetCurrentUserAsync(User.GetUserId(), cancellationToken));

    private void SetRefreshCookie(AuthResult result) =>
        Response.Cookies.Append(RefreshCookieName, result.RefreshToken, CookieOptions(result.RefreshTokenExpiresAt));

    private void ClearRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, CookieOptions(expires: null));

    // httpOnly: invisível para JavaScript (protege contra XSS). Path restrito: só vai para /api/auth.
    // SameSite=Strict funciona em dev porque localhost:5173 e localhost:5080 são o mesmo "site";
    // o deploy com domínios diferentes (Fase 17) vai exigir SameSite=None.
    private static CookieOptions CookieOptions(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = RefreshCookiePath,
        Expires = expires,
        IsEssential = true
    };
}
