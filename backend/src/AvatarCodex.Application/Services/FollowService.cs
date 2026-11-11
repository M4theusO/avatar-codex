using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>Quem pode seguir quem (Fase 24).</summary>
public static class FollowRules
{
    public const string SelfMessage = "Você não pode seguir a si mesmo.";
    public const string DemoMessage = "A conta de demonstração é compartilhada e não pode ser seguida.";

    /// <summary>
    /// Null quando pode seguir; senão, o motivo. Perfil privado de outra pessoa nem chega aqui: para quem
    /// não é o dono ele não existe (404, como no perfil público).
    /// </summary>
    public static string? CannotFollowReason(User target, Guid followerId) =>
        target.Id == followerId ? SelfMessage
        : DemoAccount.Is(target) ? DemoMessage
        : null;
}

/// <summary>Seguir e deixar de seguir, e as listas de seguindo/seguidores do próprio usuário. Seguir não dá XP.</summary>
public class FollowService(
    IUserRepository users,
    IFollowRepository follows,
    TimeProvider timeProvider,
    IValidator<PageQueryParameters> pageValidator)
{
    /// <summary>Inexistente ou privado → 404; a si mesmo ou a conta demo → 422. Seguir de novo não é erro.</summary>
    public async Task FollowAsync(Guid followerId, string username, CancellationToken cancellationToken = default)
    {
        var target = await users.GetByUsernameAsync(username.Trim(), cancellationToken);
        if (target is null || !ProfileRules.CanView(target, followerId))
            throw new NotFoundException(PublicProfileService.NotFoundMessage);

        if (FollowRules.CannotFollowReason(target, followerId) is { } reason)
            throw new BusinessRuleException(reason);

        await follows.AddAsync(followerId, target.Id, timeProvider.GetUtcNow(), cancellationToken);
    }

    /// <summary>
    /// Sempre "deu certo": deixar de seguir quem não existe ou não é seguido não faz nada. Funciona também
    /// com quem ficou privado depois, e não revela se a conta existe.
    /// </summary>
    public async Task UnfollowAsync(Guid followerId, string username, CancellationToken cancellationToken = default)
    {
        var target = await users.GetByUsernameAsync(username.Trim(), cancellationToken);
        if (target is not null)
            await follows.RemoveAsync(followerId, target.Id, cancellationToken);
    }

    public async Task<PagedResult<FollowUserDto>> GetFollowingAsync(
        Guid userId, PageQueryParameters query, CancellationToken cancellationToken = default)
    {
        await pageValidator.ValidateAndThrowAsync(query, cancellationToken);
        var (items, total) = await follows.GetFollowingAsync(userId, query.Page, query.PageSize, cancellationToken);
        return ToPage(items, total, query);
    }

    public async Task<PagedResult<FollowUserDto>> GetFollowersAsync(
        Guid userId, PageQueryParameters query, CancellationToken cancellationToken = default)
    {
        await pageValidator.ValidateAndThrowAsync(query, cancellationToken);
        var (items, total) = await follows.GetFollowersAsync(userId, query.Page, query.PageSize, cancellationToken);
        return ToPage(items, total, query);
    }

    private static PagedResult<FollowUserDto> ToPage(IReadOnlyList<FollowEntry> items, int total, PageQueryParameters query) =>
        new([.. items.Select(f => new FollowUserDto(
                f.Username,
                f.AvatarUrl,
                f.Element.ToSummaryDto(),
                LevelCalculator.GetLevel(f.ExperiencePoints),
                f.IsProfilePrivate,
                f.FollowedAt))],
            query.Page,
            query.PageSize,
            total);
}
