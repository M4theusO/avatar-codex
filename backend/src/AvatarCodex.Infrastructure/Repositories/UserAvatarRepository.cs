using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class UserAvatarRepository(AppDbContext db) : IUserAvatarRepository
{
    public Task<UserAvatar?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        db.UserAvatars.FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

    public Task<UserAvatar?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        db.UserAvatars.AsNoTracking().FirstOrDefaultAsync(a => a.PublicId == publicId, cancellationToken);

    public void Add(UserAvatar avatar) => db.UserAvatars.Add(avatar);

    public void Remove(UserAvatar avatar) => db.UserAvatars.Remove(avatar);
}
