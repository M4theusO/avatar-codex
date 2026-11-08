using AvatarCodex.Application.DTOs;

namespace AvatarCodex.Api.Extensions;

/// <summary>Cookie do refresh token, usado pelo login/refresh/logout e pela exclusão da conta.</summary>
public static class RefreshCookieExtensions
{
    public const string CookieName = "refresh_token";
    private const string CookiePath = "/api/auth";

    public static string? GetRefreshToken(this HttpRequest request) => request.Cookies[CookieName];

    public static void SetRefreshCookie(this HttpResponse response, AuthResult result) =>
        response.Cookies.Append(CookieName, result.RefreshToken, Options(result.RefreshTokenExpiresAt));

    public static void ClearRefreshCookie(this HttpResponse response) =>
        response.Cookies.Delete(CookieName, Options(expires: null));

    // httpOnly: invisível para JavaScript (protege contra XSS). Path restrito: só vai para /api/auth.
    // SameSite=Strict funciona porque o frontend e a API são o mesmo "site": em dev, localhost:5173 e
    // localhost:5080; em produção, a Vercel repassa /api/* para a API (docs/deployment.md).
    private static CookieOptions Options(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = CookiePath,
        Expires = expires,
        IsEssential = true
    };
}
