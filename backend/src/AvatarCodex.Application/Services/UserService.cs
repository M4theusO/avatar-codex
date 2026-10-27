using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>Dados agregados do usuário autenticado. Quizzes entram na Fase 9.</summary>
public class UserService(IUserCollectionRepository collection, IValidator<PageQueryParameters> pageValidator)
{
    public Task<UserStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        collection.GetStatisticsAsync(userId, cancellationToken);

    public async Task<PagedResult<ExperienceTransactionDto>> GetExperienceHistoryAsync(
        Guid userId, PageQueryParameters query, CancellationToken cancellationToken = default)
    {
        await pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var (items, totalItems) = await collection.GetExperienceAsync(userId, query.Page, query.PageSize, cancellationToken);

        return new PagedResult<ExperienceTransactionDto>(
            [.. items.Select(t => new ExperienceTransactionDto(t.Id, t.Amount, t.Type, t.Description, t.CreatedAt))],
            query.Page,
            query.PageSize,
            totalItems);
    }
}
