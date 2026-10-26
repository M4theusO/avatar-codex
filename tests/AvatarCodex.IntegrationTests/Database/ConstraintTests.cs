using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Database;

public class ConstraintTests(TestDatabase database) : IClassFixture<TestDatabase>
{
    [SkippableFact]
    public async Task User_EmailIsUniqueIgnoringCase()
    {
        database.SkipIfUnavailable();

        await using (var db = database.CreateContext())
        {
            db.Users.Add(NewUser("katara", "katara@avatar.dev"));
            await db.SaveChangesAsync();
        }

        await using var duplicate = database.CreateContext();
        duplicate.Users.Add(NewUser("katara2", "KATARA@Avatar.dev"));

        await Assert.ThrowsAsync<DbUpdateException>(() => duplicate.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task User_UsernameIsUniqueIgnoringCase()
    {
        database.SkipIfUnavailable();

        await using (var db = database.CreateContext())
        {
            db.Users.Add(NewUser("sokka", "sokka@avatar.dev"));
            await db.SaveChangesAsync();
        }

        await using var duplicate = database.CreateContext();
        duplicate.Users.Add(NewUser("SOKKA", "outro@avatar.dev"));

        await Assert.ThrowsAsync<DbUpdateException>(() => duplicate.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task User_ExperiencePointsCannotBeNegative()
    {
        database.SkipIfUnavailable();

        await using var db = database.CreateContext();
        var user = NewUser("toph", "toph@avatar.dev");
        user.ExperiencePoints = -1;
        db.Users.Add(user);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task Character_CannotRelateToItself()
    {
        database.SkipIfUnavailable();

        await using var db = database.CreateContext();
        var character = new Character
        {
            Name = "Personagem de teste",
            Description = "Descrição",
            Biography = "Biografia",
            Status = CharacterStatus.Unknown
        };
        character.Relationships.Add(new CharacterRelationship { RelatedCharacter = character, Type = RelationshipType.Friend });
        db.Characters.Add(character);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task AuditTimestamps_AreSetOnInsertAndUpdate()
    {
        database.SkipIfUnavailable();
        var clock = new ManualTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        Guid userId;
        await using (var db = database.CreateContext(clock))
        {
            var user = NewUser("zuko", "zuko@avatar.dev");
            db.Users.Add(user);
            await db.SaveChangesAsync();
            userId = user.Id;

            Assert.Equal(clock.GetUtcNow(), user.CreatedAt);
            Assert.Equal(clock.GetUtcNow(), user.UpdatedAt);

            clock.Advance(TimeSpan.FromHours(1));
            user.AvatarUrl = "https://example.com/zuko.png";
            await db.SaveChangesAsync();
        }

        await using var check = database.CreateContext();
        var saved = await check.Users.SingleAsync(u => u.Id == userId);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), saved.CreatedAt);
        Assert.Equal(new DateTimeOffset(2026, 1, 1, 1, 0, 0, TimeSpan.Zero), saved.UpdatedAt);
    }

    private static User NewUser(string username, string email) => new()
    {
        Username = username,
        Email = email,
        PasswordHash = "hash"
    };

    private sealed class ManualTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }
}
