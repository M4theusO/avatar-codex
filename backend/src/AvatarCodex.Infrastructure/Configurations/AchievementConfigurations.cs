using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.Property(a => a.Code).HasMaxLength(50);
        builder.HasIndex(a => a.Code).IsUnique();
        builder.Property(a => a.Name).HasMaxLength(100);
        builder.Property(a => a.Description).HasMaxLength(300);
        builder.Property(a => a.Icon).HasMaxLength(50);
        builder.Property(a => a.RequirementType).HasConversion<string>().HasMaxLength(30);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_achievements_requirement_value_non_negative", "requirement_value >= 0");
            // Todo desbloqueio gera uma ExperienceTransaction, que exige amount > 0.
            t.HasCheckConstraint("ck_achievements_experience_reward_positive", "experience_reward > 0");
        });
    }
}

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        // RN-05: a PK impede que a mesma conquista seja desbloqueada duas vezes.
        builder.HasKey(u => new { u.UserId, u.AchievementId });
        builder.HasIndex(u => new { u.UserId, u.UnlockedAt }).IsDescending(false, true);

        builder.HasOne<User>().WithMany().HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(u => u.Achievement).WithMany().HasForeignKey(u => u.AchievementId).OnDelete(DeleteBehavior.Restrict);
    }
}
