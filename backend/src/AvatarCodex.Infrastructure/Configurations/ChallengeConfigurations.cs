using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class ChallengeTemplateConfiguration : IEntityTypeConfiguration<ChallengeTemplate>
{
    public void Configure(EntityTypeBuilder<ChallengeTemplate> builder)
    {
        builder.Property(c => c.Code).HasMaxLength(50);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Title).HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(300);
        builder.Property(c => c.ChallengeType).HasConversion<string>().HasMaxLength(30);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_challenge_templates_target_value_positive", "target_value > 0");
            // Toda conclusão gera uma ExperienceTransaction, que exige amount > 0.
            t.HasCheckConstraint("ck_challenge_templates_experience_reward_positive", "experience_reward > 0");
        });
    }
}

public class DailyChallengeConfiguration : IEntityTypeConfiguration<DailyChallenge>
{
    public void Configure(EntityTypeBuilder<DailyChallenge> builder)
    {
        // Mesmo modelo uma vez por dia: duas gerações simultâneas do mesmo dia não duplicam (D8).
        builder.HasIndex(d => new { d.ChallengeTemplateId, d.Date }).IsUnique();
        builder.HasIndex(d => d.Date);

        builder.HasOne(d => d.Template).WithMany().HasForeignKey(d => d.ChallengeTemplateId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserDailyChallengeConfiguration : IEntityTypeConfiguration<UserDailyChallenge>
{
    public void Configure(EntityTypeBuilder<UserDailyChallenge> builder)
    {
        // RN-05: a PK impede concluir o mesmo desafio duas vezes.
        builder.HasKey(u => new { u.UserId, u.DailyChallengeId });
        builder.HasIndex(u => new { u.UserId, u.CompletedAt }).IsDescending(false, true);

        builder.HasOne<User>().WithMany().HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(u => u.DailyChallenge).WithMany().HasForeignKey(u => u.DailyChallengeId).OnDelete(DeleteBehavior.Restrict);
    }
}
