using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

/// <summary>Outra ponta de um vínculo (quem eu sigo, ou quem me segue) e desde quando.</summary>
public record FollowEntry(
    string Username,
    string? AvatarUrl,
    int ExperiencePoints,
    bool IsProfilePrivate,
    Element? Element,
    DateTimeOffset FollowedAt);

public interface IFollowRepository
{
    /// <summary>Idempotente: seguir de novo (ou em paralelo) não gera erro nem duplicata.</summary>
    Task AddAsync(Guid followerId, Guid followedId, DateTimeOffset createdAt, CancellationToken cancellationToken);

    /// <summary>Idempotente: deixar de seguir quem não é seguido não faz nada.</summary>
    Task RemoveAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken);

    Task<bool> IsFollowingAsync(Guid followerId, Guid followedId, CancellationToken cancellationToken);

    /// <summary>Quantos seguem o usuário e quantos ele segue.</summary>
    Task<(int Followers, int Following)> GetCountsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Quem o usuário segue, mais recentes primeiro.</summary>
    Task<(IReadOnlyList<FollowEntry> Items, int TotalItems)> GetFollowingAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Quem segue o usuário, mais recentes primeiro.</summary>
    Task<(IReadOnlyList<FollowEntry> Items, int TotalItems)> GetFollowersAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}
