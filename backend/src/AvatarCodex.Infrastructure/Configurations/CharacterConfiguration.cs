using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.HasIndex(c => c.Name).IsUnique();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.FirstAppearance).HasMaxLength(150);
        builder.OwnsImage(c => c.Image);

        builder.HasOne(c => c.Nation)
            .WithMany(n => n.Characters)
            .HasForeignKey(c => c.NationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Element)
            .WithMany()
            .HasForeignKey(c => c.ElementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.BendingSubtypes)
            .WithMany(b => b.Characters)
            .UsingEntity<Dictionary<string, object>>(
                "character_bending_subtypes",
                r => r.HasOne<BendingSubtype>().WithMany().HasForeignKey("BendingSubtypeId"),
                l => l.HasOne<Character>().WithMany().HasForeignKey("CharacterId"));

        builder.HasMany(c => c.Abilities)
            .WithMany(a => a.Characters)
            .UsingEntity<Dictionary<string, object>>(
                "character_abilities",
                r => r.HasOne<Ability>().WithMany().HasForeignKey("AbilityId"),
                l => l.HasOne<Character>().WithMany().HasForeignKey("CharacterId"));

        builder.HasMany(c => c.Series)
            .WithMany(s => s.Characters)
            .UsingEntity<Dictionary<string, object>>(
                "character_series",
                r => r.HasOne<Series>().WithMany().HasForeignKey("SeriesId"),
                l => l.HasOne<Character>().WithMany().HasForeignKey("CharacterId"));
    }
}
