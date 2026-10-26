using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class BendingSubtypeConfiguration : IEntityTypeConfiguration<BendingSubtype>
{
    public void Configure(EntityTypeBuilder<BendingSubtype> builder)
    {
        builder.Property(b => b.Name).HasMaxLength(80);
        builder.HasIndex(b => b.Name).IsUnique();

        builder.HasOne(b => b.Element)
            .WithMany(e => e.BendingSubtypes)
            .HasForeignKey(b => b.ElementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
