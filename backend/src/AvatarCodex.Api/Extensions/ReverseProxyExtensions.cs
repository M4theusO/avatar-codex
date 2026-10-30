using Microsoft.AspNetCore.HttpOverrides;

namespace AvatarCodex.Api.Extensions;

public static class ReverseProxyExtensions
{
    /// <summary>
    /// Em produção a API fica atrás de proxies (Vercel → Render), que terminam o HTTPS e repassam em HTTP.
    /// Lê X-Forwarded-Proto/For para a API saber o esquema e o IP originais. Os IPs dos proxies do provedor
    /// não são fixos, por isso a lista de proxies conhecidos é limpa; só o último salto é considerado.
    /// </summary>
    public static IServiceCollection AddReverseProxySupport(this IServiceCollection services) =>
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
}
