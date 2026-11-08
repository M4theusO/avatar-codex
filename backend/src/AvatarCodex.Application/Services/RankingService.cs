using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>Ranking por XP (RF-RANK-01): geral ou do mês, opcionalmente por elemento. Só para quem está logado.</summary>
public class RankingService(
    IRankingRepository repository,
    TimeProvider timeProvider,
    IValidator<RankingQueryParameters> validator)
{
    public async Task<RankingDto> GetAsync(
        Guid userId, RankingQueryParameters query, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        RankingRules.TryParsePeriod(query.Period, out var monthly);

        (DateTimeOffset Start, DateTimeOffset End)? month = monthly ? RankingRules.MonthOf(timeProvider.GetUtcNow()) : null;
        var filter = new RankingFilter(month?.Start, month?.End, query.ElementId);
        var skip = (query.Page - 1) * query.PageSize;

        var (rows, totalItems) = await repository.GetPageAsync(filter, skip, query.PageSize, cancellationToken);
        var firstPosition = rows.Count == 0 ? 0 : await repository.CountAboveAsync(filter, rows[0].Score, cancellationToken) + 1;
        var positions = RankingRules.Positions(firstPosition, skip, [.. rows.Select(r => r.Score)]);

        var entries = rows.Select((row, i) => new RankingEntryDto(
            positions[i],
            row.Username,
            LevelCalculator.GetLevel(row.TotalExperience),
            row.Score,
            row.ElementId is { } id ? new ElementSummaryDto(id, row.ElementName!, row.ElementColor!, row.ElementIcon!) : null,
            row.UserId == userId));

        return new RankingDto(
            monthly ? RankingRules.Month : RankingRules.AllTime,
            month?.Start,
            month?.End,
            query.ElementId,
            new PagedResult<RankingEntryDto>([.. entries], query.Page, query.PageSize, totalItems),
            await GetCurrentUserAsync(filter, userId, cancellationToken));
    }

    private async Task<RankingPositionDto?> GetCurrentUserAsync(
        RankingFilter filter, Guid userId, CancellationToken cancellationToken)
    {
        var score = await repository.GetScoreAsync(filter, userId, cancellationToken);
        return score is { } value
            ? new RankingPositionDto(await repository.CountAboveAsync(filter, value, cancellationToken) + 1, value)
            : null;
    }
}
