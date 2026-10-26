using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Exceptions;

namespace AvatarCodex.Application.Services;

public class NationService(INationRepository repository, IUserCollectionRepository collection)
{
    public async Task<IReadOnlyList<NationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var nations = await repository.GetAllAsync(cancellationToken);
        return [.. nations.Select(n => n.Nation.ToDto(n.CharacterCount))];
    }

    /// <param name="userId">Usuário autenticado (token opcional): preenche isFavorite/isDiscovered dos personagens.</param>
    public async Task<NationDetailsDto> GetByIdAsync(
        int id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var nation = await repository.GetDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Nação", id);

        var details = nation.ToDetailsDto();
        var marks = await collection.GetMarksOrNoneAsync(userId, [.. details.Characters.Select(c => c.Id)], cancellationToken);
        return details with { Characters = [.. details.Characters.Select(marks.Apply)] };
    }
}
