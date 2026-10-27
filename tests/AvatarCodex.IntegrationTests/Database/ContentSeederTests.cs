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

        Assert.Equal(data.Quizzes.Count, await check.Quizzes.CountAsync());
        var questions = await check.Questions.Include(q => q.Options).ToListAsync();
        Assert.Equal(data.Quizzes.Sum(q => q.Questions.Count), questions.Count);
        Assert.All(questions, q => Assert.Single(q.Options, o => o.IsCorrect));
    }

    /// <summary>xmin muda a cada UPDATE no PostgreSQL: o maior valor indica se alguma linha foi reescrita.</summary>
    private static Task<long> LastWriteAsync(Infrastructure.Data.AppDbContext db, string table) =>
        db.Database.SqlQueryRaw<long>($"SELECT max(xmin::text::bigint) AS \"Value\" FROM {table}").SingleAsync();

    [SkippableFact]
    public async Task Seed_RunTwice_DoesNotDuplicateOrModifyRows()
    {
        database.SkipIfUnavailable();
        var data = ContentSeedData.Load();

        await using (var first = database.CreateContext())
            await new ContentSeeder(first).SeedAsync(data);

        DateTimeOffset updatedAtAfterFirstRun;
        int relationshipsAfterFirstRun;
        long optionsWrittenAt;
        await using (var check = database.CreateContext())
        {
            updatedAtAfterFirstRun = await check.Characters.MaxAsync(c => c.UpdatedAt);
            relationshipsAfterFirstRun = await check.CharacterRelationships.CountAsync();
            optionsWrittenAt = await LastWriteAsync(check, "question_options");
        }

        await using (var second = database.CreateContext())
            await new ContentSeeder(second).SeedAsync(data);

        await using var final = database.CreateContext();
        Assert.Equal(data.Characters.Count, await final.Characters.CountAsync());
        Assert.Equal(relationshipsAfterFirstRun, await final.CharacterRelationships.CountAsync());
        Assert.Equal(updatedAtAfterFirstRun, await final.Characters.MaxAsync(c => c.UpdatedAt));
        Assert.Equal(optionsWrittenAt, await LastWriteAsync(final, "question_options"));
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
            Achievements = [.. data.Achievements.Select(a => a.Code == "CURIOUS" ? a with { ExperienceReward = 75 } : a)],
            Quizzes = [.. data.Quizzes.Select(q => q.Title == "O Time Avatar" ? SwapCorrectOption(q) : q)]
        };

        await using (var second = database.CreateContext())
            await new ContentSeeder(second).SeedAsync(updatedData);

        await using var check = database.CreateContext();
        var aang = await check.Characters.SingleAsync(c => c.Name == "Aang");
        Assert.Equal("Descrição atualizada", aang.Description);
        Assert.True(aang.UpdatedAt > aang.CreatedAt);
        Assert.Equal(75, (await check.Achievements.SingleAsync(a => a.Code == "CURIOUS")).ExperienceReward);
        Assert.Equal(data.Achievements.Count, await check.Achievements.CountAsync());

        // A resposta correta da 1ª pergunta mudou da 2ª para a 1ª opção sem violar "uma correta por pergunta".
        var options = await check.QuestionOptions
            .Where(o => check.Questions.Any(q => q.Id == o.QuestionId && q.Order == 1
                && check.Quizzes.Any(z => z.Id == q.QuizId && z.Title == "O Time Avatar")))
            .OrderBy(o => o.Order)
            .Select(o => o.IsCorrect)
            .ToListAsync();
        Assert.Equal([true, false, false, false], options);
    }

    private static QuizSeed SwapCorrectOption(QuizSeed quiz)
    {
        var first = quiz.Questions[0];
        var swapped = first with { Options = [.. first.Options.Select((o, i) => o with { IsCorrect = i == 0 })] };
        return quiz with { Questions = [swapped, .. quiz.Questions.Skip(1)] };
    }
}
