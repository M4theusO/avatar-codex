using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>
/// Desafios diários (RF-CHAL, D8): geração preguiçosa na primeira consulta do dia e progresso calculado
/// a partir das atividades do dia. A conclusão e o XP ficam no ProgressionService.
/// </summary>
public class DailyChallengeService(
    IChallengeRepository challenges,
    TimeProvider timeProvider,
    IValidator<PageQueryParameters> pageValidator)
{
    public async Task<DailyChallengesDto> GetTodayAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = ChallengeRules.Today(timeProvider);
        var daily = await GetOrCreateForDateAsync(today, cancellationToken);
        var completed = await challenges.GetCompletedAsync(userId, [.. daily.Select(d => d.Id)], cancellationToken);
        var activity = await GetActivityAsync(userId, today, cancellationToken);

        return new DailyChallengesDto(
            today,
            ChallengeRules.DayRange(today).To,
            [.. daily.Select(d =>
            {
                var isCompleted = completed.TryGetValue(d.Id, out var completedAt);
                return new DailyChallengeDto(
                    d.Id,
                    d.Template.Title,
                    d.Template.Description,
                    d.Template.ChallengeType,
                    d.Template.ExperienceReward,
                    // Concluído fica cheio, como as conquistas desbloqueadas.
                    isCompleted ? d.Template.TargetValue : ChallengeRules.Progress(d.Template, activity),
                    d.Template.TargetValue,
                    isCompleted,
                    isCompleted ? completedAt : null);
            })]);
    }

    /// <summary>Desafios de hoje que o usuário já cumpriu e ainda não foram registrados como concluídos.</summary>
    public async Task<IReadOnlyList<DailyChallenge>> GetMetButNotCompletedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = ChallengeRules.Today(timeProvider);
        var daily = await GetOrCreateForDateAsync(today, cancellationToken);
        var completed = await challenges.GetCompletedAsync(userId, [.. daily.Select(d => d.Id)], cancellationToken);
        var pending = daily.Where(d => !completed.ContainsKey(d.Id)).ToList();
        if (pending.Count == 0)
            return [];

        var activity = await GetActivityAsync(userId, today, cancellationToken);
        return [.. pending.Where(d => ChallengeRules.IsMet(d.Template, activity))];
    }

    public async Task<PagedResult<CompletedChallengeDto>> GetHistoryAsync(
        Guid userId, PageQueryParameters query, CancellationToken cancellationToken = default)
    {
        await pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var (items, totalItems) = await challenges.GetHistoryAsync(userId, query.Page, query.PageSize, cancellationToken);

        return new PagedResult<CompletedChallengeDto>(
            [.. items.Select(u => new CompletedChallengeDto(
                u.DailyChallengeId,
                u.DailyChallenge.Template.Title,
                u.DailyChallenge.Template.Description,
                u.DailyChallenge.Template.ExperienceReward,
                u.DailyChallenge.Date,
                u.CompletedAt))],
            query.Page,
            query.PageSize,
            totalItems);
    }

    /// <summary>Sem job agendado: o primeiro acesso do dia sorteia e grava os desafios (D8).</summary>
    private async Task<IReadOnlyList<DailyChallenge>> GetOrCreateForDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var daily = await challenges.GetForDateAsync(date, cancellationToken);
        if (daily.Count > 0)
            return daily;

        var templates = await challenges.GetActiveTemplatesAsync(cancellationToken);
        var picked = ChallengeRules.PickForDay(templates, date);
        if (picked.Count == 0)
            return [];

        await challenges.AddForDateAsync(date, [.. picked.Select(t => t.Id)], cancellationToken);
        return await challenges.GetForDateAsync(date, cancellationToken);
    }

    private Task<DailyActivity> GetActivityAsync(Guid userId, DateOnly day, CancellationToken cancellationToken)
    {
        var (from, to) = ChallengeRules.DayRange(day);
        return challenges.GetActivityAsync(userId, from, to, cancellationToken);
    }
}
