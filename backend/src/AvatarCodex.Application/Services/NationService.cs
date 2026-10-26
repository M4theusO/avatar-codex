using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Exceptions;

namespace AvatarCodex.Application.Services;

public class NationService(INationRepository repository)
{
    public async Task<IReadOnlyList<NationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var nations = await repository.GetAllAsync(cancellationToken);
        return [.. nations.Select(n => n.Nation.ToDto(n.CharacterCount))];
    }

    public async Task<NationDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var nation = await repository.GetDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Nação", id);

        return nation.ToDetailsDto();
    }
}
