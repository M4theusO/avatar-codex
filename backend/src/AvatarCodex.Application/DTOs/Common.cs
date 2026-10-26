namespace AvatarCodex.Application.DTOs;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalItems)
{
    public int TotalPages => TotalItems == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public record ImageDto(string Url, string? SourceUrl, string? Credit);

public record ReferenceDto(int Id, string Name);

public record ElementSummaryDto(int Id, string Name, string Color, string Icon);
