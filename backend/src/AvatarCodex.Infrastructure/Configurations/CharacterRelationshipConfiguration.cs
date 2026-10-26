using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class CharacterRelationshipConfiguration : IEntityTypeConfiguration<CharacterRelationship>
{
    public void Configure(EntityTypeBuilder<CharacterRelationship> builder)
    {
        builder.HasKey(r => new { r.CharacterId, r.RelatedCharacterId, r.Type });
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Description).HasMaxLength(200);

        builder.HasOne(r => r.Character)
            .WithMany(c => c.Relationships)
            .HasForeignKey(r => r.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.RelatedCharacter)
            .WithMany()
            .HasForeignKey(r => r.RelatedCharacterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_character_relationships_not_self", "character_id <> related_character_id"));
    }
}
