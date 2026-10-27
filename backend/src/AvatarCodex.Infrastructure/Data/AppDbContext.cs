using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, TimeProvider timeProvider) : DbContext(options)
{
    public DbSet<Element> Elements => Set<Element>();
    public DbSet<BendingSubtype> BendingSubtypes => Set<BendingSubtype>();
    public DbSet<Nation> Nations => Set<Nation>();
    public DbSet<Series> Series => Set<Series>();
    public DbSet<Ability> Abilities => Set<Ability>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<CharacterRelationship> CharacterRelationships => Set<CharacterRelationship>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserCharacterDiscovery> CharacterDiscoveries => Set<UserCharacterDiscovery>();
    public DbSet<UserNationDiscovery> NationDiscoveries => Set<UserNationDiscovery>();
    public DbSet<UserFavoriteCharacter> FavoriteCharacters => Set<UserFavoriteCharacter>();
    public DbSet<ExperienceTransaction> ExperienceTransactions => Set<ExperienceTransaction>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<UserQuizAttempt> QuizAttempts => Set<UserQuizAttempt>();
    public DbSet<UserQuizAnswer> QuizAnswers => Set<UserQuizAnswer>();
    public DbSet<ChallengeTemplate> ChallengeTemplates => Set<ChallengeTemplate>();
    public DbSet<DailyChallenge> DailyChallenges => Set<DailyChallenge>();
    public DbSet<UserDailyChallenge> UserDailyChallenges => Set<UserDailyChallenge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void SetAuditTimestamps()
    {
        var now = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
