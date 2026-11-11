using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class UserFollowConfiguration : IEntityTypeConfiguration<UserFollow>
{
    public void Configure(EntityTypeBuilder<UserFollow> builder)
    {
        builder.HasKey(f => new { f.FollowerId, f.FollowedId });
        // Lista de seguidores e contagem: busca pelo seguido.
        builder.HasIndex(f => new { f.FollowedId, f.CreatedAt }).IsDescending(false, true);

        builder.HasOne(f => f.Follower).WithMany().HasForeignKey(f => f.FollowerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.Followed).WithMany().HasForeignKey(f => f.FollowedId).OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_user_follows_not_self", "follower_id <> followed_id"));
    }
}
