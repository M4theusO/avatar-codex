using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // citext: unicidade sem diferenciar maiúsculas/minúsculas (RN-01)
        builder.Property(u => u.Username).HasColumnType("citext").HasMaxLength(30);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.Email).HasColumnType("citext").HasMaxLength(254);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.ExperiencePoints).HasDefaultValue(0);

        builder.HasOne(u => u.Element)
            .WithMany()
            .HasForeignKey(u => u.ElementId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_users_experience_points_non_negative", "experience_points >= 0"));
    }
}
