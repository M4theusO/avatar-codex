using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AvatarCodex.Infrastructure.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.Property(q => q.Title).HasMaxLength(150);
        builder.HasIndex(q => q.Title).IsUnique();
        builder.Property(q => q.Category).HasMaxLength(50);
        builder.Property(q => q.Difficulty).HasConversion<string>().HasMaxLength(20);
        builder.OwnsImage(q => q.Image);

        builder.HasMany(q => q.Questions).WithOne().HasForeignKey(q => q.QuizId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.Property(q => q.Text).HasMaxLength(500);
        builder.Property(q => q.ImageUrl).HasMaxLength(500);
        builder.Property(q => q.Explanation).HasMaxLength(500);
        builder.HasIndex(q => new { q.QuizId, q.Order }).IsUnique();

        builder.HasMany(q => q.Options).WithOne().HasForeignKey(o => o.QuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.Property(o => o.Text).HasMaxLength(300);
        builder.HasIndex(o => new { o.QuestionId, o.Order }).IsUnique();

        // RN-09: no máximo uma opção correta por pergunta (o seed garante que existe uma).
        builder.HasIndex(o => o.QuestionId)
            .IsUnique()
            .HasFilter("is_correct")
            .HasDatabaseName("ix_question_options_question_id_single_correct");
    }
}

public class UserQuizAttemptConfiguration : IEntityTypeConfiguration<UserQuizAttempt>
{
    public void Configure(EntityTypeBuilder<UserQuizAttempt> builder)
    {
        builder.HasIndex(a => new { a.UserId, a.CompletedAt }).IsDescending(false, true);

        builder.HasOne<User>().WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Quiz).WithMany().HasForeignKey(a => a.QuizId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(a => a.Answers).WithOne().HasForeignKey(a => a.UserQuizAttemptId).OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_user_quiz_attempts_score_range", "score >= 0 AND score <= total_questions");
            t.HasCheckConstraint("ck_user_quiz_attempts_experience_non_negative", "experience_earned >= 0");
        });
    }
}

public class UserQuizAnswerConfiguration : IEntityTypeConfiguration<UserQuizAnswer>
{
    public void Configure(EntityTypeBuilder<UserQuizAnswer> builder)
    {
        builder.HasKey(a => new { a.UserQuizAttemptId, a.QuestionId });

        builder.HasOne<Question>().WithMany().HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<QuestionOption>().WithMany().HasForeignKey(a => a.SelectedOptionId).OnDelete(DeleteBehavior.Restrict);
    }
}
