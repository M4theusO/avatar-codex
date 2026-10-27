using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AvatarCodex.Infrastructure.Repositories;

public class UserCollectionRepository(AppDbContext db) : IUserCollectionRepository
{
    public async Task<UserMarks> GetMarksAsync(
        Guid userId, IReadOnlyCollection<int> characterIds, CancellationToken cancellationToken)
    {
        var favorites = await db.FavoriteCharacters
            .Where(f => f.UserId == userId && characterIds.Contains(f.CharacterId))
            .Select(f => f.CharacterId)
            .ToListAsync(cancellationToken);

        var discovered = await db.CharacterDiscoveries
            .Where(d => d.UserId == userId && characterIds.Contains(d.CharacterId))
            .Select(d => d.CharacterId)
            .ToListAsync(cancellationToken);

        return new UserMarks(favorites.ToHashSet(), discovered.ToHashSet());
    }

    public async Task AddFavoriteAsync(Guid userId, int characterId, DateTimeOffset createdAt, CancellationToken cancellationToken)
    {
        if (await db.FavoriteCharacters.AnyAsync(f => f.UserId == userId && f.CharacterId == characterId, cancellationToken))
            return;

        db.FavoriteCharacters.Add(new UserFavoriteCharacter { UserId = userId, CharacterId = characterId, CreatedAt = createdAt });
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Favoritado em paralelo por outra requisição: o resultado é o mesmo.
            db.ChangeTracker.Clear();
        }
    }

    public Task RemoveFavoriteAsync(Guid userId, int characterId, CancellationToken cancellationToken) =>
        db.FavoriteCharacters
            .Where(f => f.UserId == userId && f.CharacterId == characterId)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task<(IReadOnlyList<Character> Items, int TotalItems)> GetFavoritesAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var favorites = db.FavoriteCharacters.AsNoTracking().Where(f => f.UserId == userId);

        var totalItems = await favorites.CountAsync(cancellationToken);
        var items = await favorites
            .Include(f => f.Character).ThenInclude(c => c.Nation)
            .Include(f => f.Character).ThenInclude(c => c.Element)
            .OrderByDescending(f => f.CreatedAt)
            .ThenBy(f => f.CharacterId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ([.. items.Select(f => f.Character)], totalItems);
    }

    public async Task<IReadOnlyList<Character>> GetUndiscoveredCharactersAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Characters
            .AsNoTracking()
            .Include(c => c.Nation)
            .Include(c => c.Element)
            .Where(c => !db.CharacterDiscoveries.Any(d => d.UserId == userId && d.CharacterId == c.Id))
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<DiscoveryEntry> Items, int TotalItems)> GetDiscoveriesAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        // O conteúdo é pequeno (dezenas de itens por usuário): juntar as duas listas em memória
        // é mais simples que um UNION com projeção de tipos owned.
        var characters = await db.CharacterDiscoveries
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Select(d => new DiscoveryEntry(
                DiscoveryKind.Character, d.CharacterId, d.Character.Name, d.Character.Image, d.Character.Element, d.DiscoveredAt))
            .ToListAsync(cancellationToken);

        var nations = await db.NationDiscoveries
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Select(d => new DiscoveryEntry(
                DiscoveryKind.Nation, d.NationId, d.Nation.Name, d.Nation.SymbolImage, d.Nation.Element, d.DiscoveredAt))
            .ToListAsync(cancellationToken);

        var all = characters.Concat(nations)
            .OrderByDescending(d => d.DiscoveredAt)
            .ThenBy(d => d.Kind)
            .ThenBy(d => d.Id)
            .ToList();

        return ([.. all.Skip((page - 1) * pageSize).Take(pageSize)], all.Count);
    }

    public async Task<(IReadOnlyList<ExperienceTransaction> Items, int TotalItems)> GetExperienceAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var transactions = db.ExperienceTransactions.AsNoTracking().Where(t => t.UserId == userId);

        var totalItems = await transactions.CountAsync(cancellationToken);
        var items = await transactions
            .OrderByDescending(t => t.CreatedAt)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<UserStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var attempts = db.QuizAttempts.Where(a => a.UserId == userId);
        var answered = await attempts.SumAsync(a => a.TotalQuestions, cancellationToken);
        var correct = await attempts.SumAsync(a => a.Score, cancellationToken);

        return new(
            DiscoveredCharacters: await db.CharacterDiscoveries.CountAsync(d => d.UserId == userId, cancellationToken),
            TotalCharacters: await db.Characters.CountAsync(cancellationToken),
            DiscoveredNations: await db.NationDiscoveries.CountAsync(d => d.UserId == userId, cancellationToken),
            TotalNations: await db.Nations.CountAsync(cancellationToken),
            Favorites: await db.FavoriteCharacters.CountAsync(f => f.UserId == userId, cancellationToken),
            UnlockedAchievements: await db.UserAchievements.CountAsync(a => a.UserId == userId, cancellationToken),
            TotalAchievements: await db.Achievements.CountAsync(cancellationToken),
            CompletedQuizzes: await attempts.Select(a => a.QuizId).Distinct().CountAsync(cancellationToken),
            TotalQuizzes: await db.Quizzes.CountAsync(cancellationToken),
            QuizAttempts: await attempts.CountAsync(cancellationToken),
            AccuracyPercent: answered == 0 ? null : QuizScoring.Percentage(correct, answered));
    }
}
