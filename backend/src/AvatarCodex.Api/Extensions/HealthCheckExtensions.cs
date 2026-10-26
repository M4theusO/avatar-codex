using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace AvatarCodex.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IEndpointConventionBuilder MapApiHealthChecks(this IEndpointRouteBuilder app) =>
        app.MapHealthChecks("/api/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString())
                }, JsonSerializerOptions.Web));
            }
        });
}
