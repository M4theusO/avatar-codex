using AvatarCodex.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.IntegrationTests.Database;

public class ContentSeederTests(TestDatabase database) : IClassFixture<TestDatabase>
{
    [SkippableFact]
    public async Task Seed_PopulatesEmptyDatabase()
    {
        database.SkipIfUnavailable();
        var data = ContentSeedData.Load();

        await using (var db = database.CreateContext())
            await new ContentSeeder(db).SeedAsync(data);

        await using var check = database.CreateContext();
        Assert.Equal(data.Elements.Count, await check.Elements.CountAsync());
        Assert.Equal(data.Nations.Count, await check.Nations.CountAsync());
        Assert.Equal(data.Characters.Count, await check.Characters.CountAsync());

        var zuko = await check.Characters
            .Include(c => c.Nation)
            .Include(c => c.Element)
            .Include(c => c.Abilities)
            .Include(c => c.Relationships).ThenInclude(r => r.RelatedCharacter)
            .SingleAsync(c => c.Name == "Zuko");

        Assert.Equal("Nação do Fogo", zuko.Nation?.Name);
        Assert.Equal("Fogo", zuko.Element?.Name);
        Assert.Contains(zuko.Abilities, a => a.Name == "Redirecionamento de relâmpago");
        Assert.Contains(zuko.Relationships, r => r.RelatedCharacter.Name == "Iroh" && r.Description == "Tio");
    }

    [SkippableFact]
    public async Task Seed_RunTwice_DoesNotDuplicateOrModifyRows()
    {
        database.SkipIfUnavailable();
        var data = ContentSeedData.Load();

        await using (var first = database.CreateContext())
            await new ContentSeeder(first).SeedAsync(data);

        DateTimeOffset updatedAtAfterFirstRun;
        int relationshipsAfterFirstRun;
        await using (var check = database.CreateContext())
        {
            updatedAtAfterFirstRun = await check.Characters.MaxAsync(c => c.UpdatedAt);
            relationshipsAfterFirstRun = await check.CharacterRelationships.CountAsync();
        }

        await using (var second = database.CreateContext())
            await new ContentSeeder(second).SeedAsync(data);

        await using var final = database.CreateContext();
        Assert.Equal(data.Characters.Count, await final.Characters.CountAsync());
        Assert.Equal(relationshipsAfterFirstRun, await final.CharacterRelationships.CountAsync());
        Assert.Equal(updatedAtAfterFirstRun, await final.Characters.MaxAsync(c => c.UpdatedAt));
    }

    [SkippableFact]
    public async Task Seed_UpdatesChangedContent()
    {
        database.SkipIfUnavailable();
        var data = ContentSeedData.Load();

        await using (var first = database.CreateContext())
            await new ContentSeeder(first).SeedAsync(data);

        var changed = data.Characters.Single(c => c.Name == "Aang") with { Description = "Descrição atualizada" };
        var updatedData = new ContentSeedData
        {
            Elements = data.Elements,
            BendingSubtypes = data.BendingSubtypes,
            Nations = data.Nations,
            Series = data.Series,
            Abilities = data.Abilities,
            Characters = [.. data.Characters.Select(c => c.Name == "Aang" ? changed : c)],
            Achievements = [.. data.Achievements.Select(a => a.Code == "CURIOUS" ? a with { ExperienceReward = 75 } : a)]
        };

        await using (var second = database.CreateContext())
            await new ContentSeeder(second).SeedAsync(updatedData);

        await using var check = database.CreateContext();
        var aang = await check.Characters.SingleAsync(c => c.Name == "Aang");
        Assert.Equal("Descrição atualizada", aang.Description);
        Assert.True(aang.UpdatedAt > aang.CreatedAt);
        Assert.Equal(75, (await check.Achievements.SingleAsync(a => a.Code == "CURIOUS")).ExperienceReward);
        Assert.Equal(data.Achievements.Count, await check.Achievements.CountAsync());
    }
}
