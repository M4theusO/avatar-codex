using AvatarCodex.Application.DTOs;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.ValueObjects;

namespace AvatarCodex.Application.Interfaces;

public interface IProgressionRepository
{
    /// <summary>
    /// Grava o registro da atividade (ex.: a descoberta; opcional) e a ExperienceTransaction numa única transação
    /// e soma o XP ao total do usuário. Retorna o novo total, ou null se a atividade/recompensa já
    /// existia: a violação de unicidade significa "já concedido" (RN-05, RN-12).
    /// </summary>
    Task<int?> SaveWithExperienceAsync(object? activityRecord, ExperienceTransaction transaction, CancellationToken cancellationToken);
}

/// <summary>Quais personagens de uma lista o usuário favoritou e descobriu.</summary>
public record UserMarks(IReadOnlySet<int> FavoriteIds, IReadOnlySet<int> DiscoveredIds)
{
    public static readonly UserMarks None = new(new HashSet<int>(), new HashSet<int>());

    public CharacterListItemDto Apply(CharacterListItemDto character) => character with
    {
        IsFavorite = FavoriteIds.Contains(character.Id),
        IsDiscovered = DiscoveredIds.Contains(character.Id)
    };

    public CharacterDetailsDto Apply(CharacterDetailsDto character) => character with
    {
        IsFavorite = FavoriteIds.Contains(character.Id),
        IsDiscovered = DiscoveredIds.Contains(character.Id)
    };
}

public record DiscoveryEntry(
    DiscoveryKind Kind,
    int Id,
    string Name,
    ImageInfo? Image,
    Element? Element,
    DateTimeOffset DiscoveredAt);

/// <summary>Favoritos, descobertas e contagens do usuário.</summary>
public interface IUserCollectionRepository
{
    Task<UserMarks> GetMarksAsync(Guid userId, IReadOnlyCollection<int> characterIds, CancellationToken cancellationToken);

    /// <summary>Idempotente: favoritar de novo (ou em paralelo) não gera erro nem duplicata.</summary>
    Task AddFavoriteAsync(Guid userId, int characterId, DateTimeOffset createdAt, CancellationToken cancellationToken);

    Task RemoveFavoriteAsync(Guid userId, int characterId, CancellationToken cancellationToken);

    /// <summary>Favoritos mais recentes primeiro, com Nation e Element carregados.</summary>
    Task<(IReadOnlyList<Character> Items, int TotalItems)> GetFavoritesAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Descobertas de personagens e nações, mais recentes primeiro.</summary>
    Task<(IReadOnlyList<DiscoveryEntry> Items, int TotalItems)> GetDiscoveriesAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);

    Task<UserStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Movimentações de XP, mais recentes primeiro.</summary>
    Task<(IReadOnlyList<ExperienceTransaction> Items, int TotalItems)> GetExperienceAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}

public static class UserCollectionRepositoryExtensions
{
    /// <summary>Marcas do usuário, ou nenhuma para visitantes (endpoints com token opcional).</summary>
    public static Task<UserMarks> GetMarksOrNoneAsync(
        this IUserCollectionRepository collection,
        Guid? userId,
        IReadOnlyCollection<int> characterIds,
        CancellationToken cancellationToken) =>
        userId is null || characterIds.Count == 0
            ? Task.FromResult(UserMarks.None)
            : collection.GetMarksAsync(userId.Value, characterIds, cancellationToken);
}
