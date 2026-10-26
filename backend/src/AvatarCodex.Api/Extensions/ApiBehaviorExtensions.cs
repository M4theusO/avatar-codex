using System.Text.Json.Serialization;
using AvatarCodex.Api.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace AvatarCodex.Api.Extensions;

public static class ApiBehaviorExtensions
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(options =>
            {
                // Erros de binding (ex. ?page=abc) no mesmo formato dos erros de validação.
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .Select(entry => new FieldError(
                            GlobalExceptionHandler.ToCamelCase(entry.Key),
                            "Valor inválido."))
                        .ToList();

                    return new BadRequestObjectResult(
                        new ErrorResponse(StatusCodes.Status400BadRequest, GlobalExceptionHandler.InvalidDataMessage, errors));
                };
            });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
