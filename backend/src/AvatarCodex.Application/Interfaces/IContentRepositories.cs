using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Interfaces;

public enum CharacterType
{
    Avatar,
    Bender,
    NonBender
}

public enum CharacterSortField
{
    Name,
    Nation,
    Element
}

/// <summary>Filtro já validado e convertido, usado pelo repositório de personagens.</summary>
public record CharacterFilter(
    int Page,
    int PageSize,
    string? Search = null,
    int? NationId = null,
    int? ElementId = null,
    CharacterType? Type = null,
    CharacterStatus? Status = null,
    int? SeriesId = null,
    CharacterSortField Sort = CharacterSortField.Name,
    bool Descending = false);

public interface ICharacterRepository
{
    /// <summary>Página de personagens com Nation e Element carregados.</summary>
    Task<(IReadOnlyList<Character> Items, int TotalItems)> GetPagedAsync(CharacterFilter filter, CancellationToken cancellationToken);

    /// <summary>Personagem com todas as associações carregadas, ou null.</summary>
    Task<Character?> GetDetailsAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    /// <summary>Nome do personagem, ou null se não existir.</summary>
    Task<string?> GetNameAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Ability>> GetAbilitiesAsync(int characterId, CancellationToken cancellationToken);
}

public interface INationRepository
{
    Task<IReadOnlyList<(Nation Nation, int CharacterCount)>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Nação com Element e personagens (com Nation e Element) carregados, ou null.</summary>
    Task<Nation?> GetDetailsAsync(int id, CancellationToken cancellationToken);

    /// <summary>Nome da nação, ou null se não existir.</summary>
    Task<string?> GetNameAsync(int id, CancellationToken cancellationToken);
}

public interface ICatalogRepository
{
    Task<IReadOnlyList<Element>> GetElementsAsync(CancellationToken cancellationToken);

    /// <summary>Habilidades na ordem dos elementos (sem elemento por último) e por nome, com a contagem de personagens.</summary>
    Task<IReadOnlyList<(Ability Ability, int CharacterCount)>> GetAbilitiesAsync(
        int? elementId, CancellationToken cancellationToken);

    /// <summary>Habilidade com Element e personagens (com Nation e Element) carregados, ou null.</summary>
    Task<Ability?> GetAbilityDetailsAsync(int id, CancellationToken cancellationToken);

    /// <summary>Nome da habilidade, ou null se não existir.</summary>
    Task<string?> GetAbilityNameAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Series>> GetSeriesAsync(CancellationToken cancellationToken);
}
