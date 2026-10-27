using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Mappings;

public static class UserMappings
{
    /// <summary>Resumo do usuário com o progresso do nível já calculado (RN-07 só no backend).</summary>
    public static UserDto ToDto(this User user)
    {
        var progress = LevelCalculator.GetProgress(user.ExperiencePoints);
        return new UserDto(
            user.Id,
            user.Username,
            user.Email,
            user.AvatarUrl,
            user.Element.ToSummaryDto(),
            progress.Level,
            user.ExperiencePoints,
            progress.CurrentLevelXp,
            progress.NextLevelXp,
            progress.ProgressPercent,
            user.CreatedAt);
    }

    public static DiscoveryDto ToDto(this DiscoveryEntry discovery) => new(
        discovery.Kind,
        discovery.Id,
        discovery.Name,
        discovery.Image.ToDto(),
        discovery.Element.ToSummaryDto(),
        discovery.DiscoveredAt);
}
