namespace AvatarCodex.Api.Middleware;

/// <summary>Formato padrão de erro da API (docs/api.md §3).</summary>
public record ErrorResponse(int Status, string Message, IReadOnlyList<FieldError> Errors);

public record FieldError(string Field, string Message);
