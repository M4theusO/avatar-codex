using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;

namespace AvatarCodex.Application.Services;

/// <summary>Dados agregados do usuário autenticado. Quizzes e conquistas entram nas próximas fases.</summary>
public class UserService(IUserCollectionRepository collection)
{
    public Task<UserStatisticsDto> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        collection.GetStatisticsAsync(userId, cancellationToken);
}
