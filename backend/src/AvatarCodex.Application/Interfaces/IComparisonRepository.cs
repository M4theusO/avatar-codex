using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

public interface IComparisonRepository
{
    /// <summary>Atributos de comparação na ordem de exibição.</summary>
    Task<IReadOnlyList<ComparisonAttribute>> GetAttributesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Personagens encontrados entre os ids (em qualquer ordem), com nação, elemento, subtipos,
    /// habilidades, séries e valores dos atributos carregados.
    /// </summary>
    Task<IReadOnlyList<Character>> GetCharactersAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
}
