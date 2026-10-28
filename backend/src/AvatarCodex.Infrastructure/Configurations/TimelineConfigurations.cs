using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class EraConfiguration : IEntityTypeConfiguration<Era>
{
    public void Configure(EntityTypeBuilder<Era> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(100);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.Order).IsUnique();
    }
}

public class TimelineEventConfiguration : IEntityTypeConfiguration<TimelineEvent>
{
    public void Configure(EntityTypeBuilder<TimelineEvent> builder)
    {
        builder.Property(e => e.Title).HasMaxLength(150);
        builder.HasIndex(e => e.Title).IsUnique();
        builder.Property(e => e.DateLabel).HasMaxLength(50);
        builder.HasIndex(e => e.Order).IsUnique();
        builder.OwnsImage(e => e.Image);

        builder.HasOne(e => e.Era)
            .WithMany(era => era.Events)
            .HasForeignKey(e => e.EraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Characters)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "timeline_event_characters",
                r => r.HasOne<Character>().WithMany().HasForeignKey("CharacterId").OnDelete(DeleteBehavior.Restrict),
                l => l.HasOne<TimelineEvent>().WithMany().HasForeignKey("TimelineEventId").OnDelete(DeleteBehavior.Cascade));
    }
}
