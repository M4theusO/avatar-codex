using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class AbilityConfiguration : IEntityTypeConfiguration<Ability>
{
    public void Configure(EntityTypeBuilder<Ability> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(100);
        builder.HasIndex(a => a.Name).IsUnique();
        builder.OwnsImage(a => a.Image);

        builder.HasOne(a => a.Element)
            .WithMany()
            .HasForeignKey(a => a.ElementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
