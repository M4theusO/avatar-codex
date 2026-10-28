using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class ComparisonAttributeConfiguration : IEntityTypeConfiguration<ComparisonAttribute>
{
    public void Configure(EntityTypeBuilder<ComparisonAttribute> builder)
    {
        builder.Property(a => a.Name).HasMaxLength(50);
        builder.HasIndex(a => a.Name).IsUnique();
        builder.Property(a => a.Description).HasMaxLength(200);
    }
}

public class CharacterAttributeValueConfiguration : IEntityTypeConfiguration<CharacterAttributeValue>
{
    public void Configure(EntityTypeBuilder<CharacterAttributeValue> builder)
    {
        builder.HasKey(v => new { v.CharacterId, v.ComparisonAttributeId });

        builder.HasOne(v => v.Character)
            .WithMany(c => c.AttributeValues)
            .HasForeignKey(v => v.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.ComparisonAttribute)
            .WithMany()
            .HasForeignKey(v => v.ComparisonAttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("ck_character_attribute_values_value_range", "value >= 0 AND value <= 100"));
    }
}
