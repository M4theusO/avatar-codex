using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class ElementConfiguration : IEntityTypeConfiguration<Element>
{
    public void Configure(EntityTypeBuilder<Element> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(50);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.Color).HasMaxLength(7).IsFixedLength();
        builder.Property(e => e.Icon).HasMaxLength(50);
    }
}
