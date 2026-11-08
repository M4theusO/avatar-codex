using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class UserAvatarConfiguration : IEntityTypeConfiguration<UserAvatar>
{
    public void Configure(EntityTypeBuilder<UserAvatar> builder)
    {
        // PK = UserId: no máximo uma foto por usuário; enviar outra sobrescreve a linha.
        builder.HasKey(a => a.UserId);
        builder.HasIndex(a => a.PublicId).IsUnique();
        builder.Property(a => a.ContentType).HasMaxLength(20);

        builder.HasOne(a => a.User)
            .WithOne()
            .HasForeignKey<UserAvatar>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
