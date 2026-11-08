using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

public interface IElementTestRepository
{
    /// <summary>Perguntas na ordem, com as opções (e o elemento de cada uma) na ordem.</summary>
    Task<IReadOnlyList<ElementTestQuestion>> GetQuestionsAsync(CancellationToken cancellationToken);

    /// <summary>Elementos na ordem do catálogo, com os subtipos (para o resultado).</summary>
    Task<IReadOnlyList<Element>> GetElementsAsync(CancellationToken cancellationToken);

    Task SetUserElementAsync(Guid userId, int elementId, CancellationToken cancellationToken);
}
