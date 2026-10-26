using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class NationConfiguration : IEntityTypeConfiguration<Nation>
{
    public void Configure(EntityTypeBuilder<Nation> builder)
    {
        builder.Property(n => n.Name).HasMaxLength(100);
        builder.HasIndex(n => n.Name).IsUnique();
        builder.OwnsImage(n => n.SymbolImage);

        builder.HasOne(n => n.Element)
            .WithMany()
            .HasForeignKey(n => n.ElementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
