using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Interfaces;

/// <summary>Fotos de perfil (uma por usuário). Usa o mesmo contexto do IUserRepository: um SaveChanges grava os dois.</summary>
public interface IUserAvatarRepository
{
    /// <summary>Foto do usuário (rastreada, para substituir ou remover), ou null.</summary>
    Task<UserAvatar?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Foto pelo id público da URL (sem rastreamento), ou null.</summary>
    Task<UserAvatar?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    void Add(UserAvatar avatar);

    void Remove(UserAvatar avatar);
}
