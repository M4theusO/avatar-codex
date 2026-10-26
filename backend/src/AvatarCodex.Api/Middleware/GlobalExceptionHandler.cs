using System.Text.Json;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace AvatarCodex.Api.Middleware;

/// <summary>Converte exceções no formato padrão de erro, sem expor detalhes internos.</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public const string InvalidDataMessage = "Os dados enviados são inválidos.";

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var response = exception switch
        {
            ValidationException validation => new ErrorResponse(
                StatusCodes.Status400BadRequest,
                InvalidDataMessage,
                [.. validation.Errors.Select(e => new FieldError(ToCamelCase(e.PropertyName), e.ErrorMessage))]),
            UnauthorizedException => new ErrorResponse(StatusCodes.Status401Unauthorized, exception.Message, []),
            NotFoundException => new ErrorResponse(StatusCodes.Status404NotFound, exception.Message, []),
            ConflictException => new ErrorResponse(StatusCodes.Status409Conflict, exception.Message, []),
            BusinessRuleException => new ErrorResponse(StatusCodes.Status422UnprocessableEntity, exception.Message, []),
            _ => null
        };

        if (response is null)
        {
            logger.LogError(exception, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);
            response = new ErrorResponse(StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado. Tente novamente mais tarde.", []);
        }

        context.Response.StatusCode = response.Status;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }

    public static string ToCamelCase(string propertyName) => JsonNamingPolicy.CamelCase.ConvertName(propertyName);
}
