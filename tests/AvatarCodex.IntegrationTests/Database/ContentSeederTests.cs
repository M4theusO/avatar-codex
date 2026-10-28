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

        Assert.Equal(data.ComparisonAttributes.Count, await check.ComparisonAttributes.CountAsync());
        Assert.Equal(
            data.CharacterAttributes.Sum(c => c.Values.Count),
            await check.CharacterAttributeValues.CountAsync());
        var sokkaBending = await check.CharacterAttributeValues
            .SingleAsync(v => v.Character.Name == "Sokka" && v.ComparisonAttribute.Name == "Domínio elemental");
        Assert.Equal(0, sokkaBending.Value);
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
        long attributeValuesWrittenAt;
        await using (var check = database.CreateContext())
        {
            updatedAtAfterFirstRun = await check.Characters.MaxAsync(c => c.UpdatedAt);
            relationshipsAfterFirstRun = await check.CharacterRelationships.CountAsync();
            optionsWrittenAt = await LastWriteAsync(check, "question_options");
            attributeValuesWrittenAt = await LastWriteAsync(check, "character_attribute_values");
        }

        await using (var second = database.CreateContext())
            await new ContentSeeder(second).SeedAsync(data);

        await using var final = database.CreateContext();
        Assert.Equal(data.Characters.Count, await final.Characters.CountAsync());
        Assert.Equal(relationshipsAfterFirstRun, await final.CharacterRelationships.CountAsync());
        Assert.Equal(updatedAtAfterFirstRun, await final.Characters.MaxAsync(c => c.UpdatedAt));
        Assert.Equal(optionsWrittenAt, await LastWriteAsync(final, "question_options"));
        Assert.Equal(attributeValuesWrittenAt, await LastWriteAsync(final, "character_attribute_values"));
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
            Quizzes = [.. data.Quizzes.Select(q => q.Title == "O Time Avatar" ? SwapCorrectOption(q) : q)],
            Challenges = [.. data.Challenges.Select(c => c.Code == "QUIZ_PERFECT" ? c with { IsActive = false } : c)],
            Eras = data.Eras,
            // Troca a ordem de dois eventos da mesma era e muda os personagens de um deles.
            TimelineEvents = SwapFirstTwoEventsOfKorra(data.TimelineEvents),
            // Inverte a ordem dos atributos e muda um valor do Aang.
            ComparisonAttributes = [.. Enumerable.Reverse(data.ComparisonAttributes)],
            CharacterAttributes = [.. data.CharacterAttributes.Select(c => c.Character == "Aang"
                ? c with { Values = new(c.Values) { ["Estratégia"] = 99 } }
                : c)]
        };

        await using (var second = database.CreateContext())
            await new ContentSeeder(second).SeedAsync(updatedData);

        await using var check = database.CreateContext();
        var aang = await check.Characters.SingleAsync(c => c.Name == "Aang");
        Assert.Equal("Descrição atualizada", aang.Description);
        Assert.True(aang.UpdatedAt > aang.CreatedAt);
        Assert.Equal(75, (await check.Achievements.SingleAsync(a => a.Code == "CURIOUS")).ExperienceReward);
        Assert.Equal(data.Achievements.Count, await check.Achievements.CountAsync());
        Assert.False((await check.ChallengeTemplates.SingleAsync(c => c.Code == "QUIZ_PERFECT")).IsActive);
        Assert.Equal(data.Challenges.Count, await check.ChallengeTemplates.CountAsync());

        var korraEvents = await check.TimelineEvents
            .Where(e => e.Era.Name == "A Era de Korra")
            .OrderBy(e => e.Order)
            .Select(e => new { e.Title, Characters = e.Characters.Select(c => c.Name).ToList() })
            .ToListAsync();
        Assert.Equal("A morte de Aang e o nascimento de Korra", korraEvents[0].Title);
        Assert.Equal("Nasce a Cidade República", korraEvents[1].Title);
        Assert.Equal(["Aang"], korraEvents[1].Characters);
        Assert.Equal(data.TimelineEvents.Count, await check.TimelineEvents.CountAsync());
        Assert.Equal(
            Enumerable.Range(1, data.TimelineEvents.Count),
            await check.TimelineEvents.OrderBy(e => e.Order).Select(e => e.Order).ToListAsync());

        // A resposta correta da 1ª pergunta mudou da 2ª para a 1ª opção sem violar "uma correta por pergunta".
        var options = await check.QuestionOptions
            .Where(o => check.Questions.Any(q => q.Id == o.QuestionId && q.Order == 1
                && check.Quizzes.Any(z => z.Id == q.QuizId && z.Title == "O Time Avatar")))
            .OrderBy(o => o.Order)
            .Select(o => o.IsCorrect)
            .ToListAsync();
        Assert.Equal([true, false, false, false], options);

        Assert.Equal(
            data.ComparisonAttributes.Select(a => a.Name).Reverse(),
            await check.ComparisonAttributes.OrderBy(a => a.Order).Select(a => a.Name).ToListAsync());
        var aangStrategy = await check.CharacterAttributeValues
            .SingleAsync(v => v.Character.Name == "Aang" && v.ComparisonAttribute.Name == "Estratégia");
        Assert.Equal(99, aangStrategy.Value);
        Assert.Equal(
            data.CharacterAttributes.Sum(c => c.Values.Count),
            await check.CharacterAttributeValues.CountAsync());
    }

    private static List<TimelineEventSeed> SwapFirstTwoEventsOfKorra(List<TimelineEventSeed> events)
    {
        var result = events.ToList();
        var first = result.FindIndex(e => e.Era == "A Era de Korra");
        (result[first], result[first + 1]) = (result[first + 1], result[first] with { Characters = ["Aang"] });
        return result;
    }

    private static QuizSeed SwapCorrectOption(QuizSeed quiz)
    {
        var first = quiz.Questions[0];
        var swapped = first with { Options = [.. first.Options.Select((o, i) => o with { IsCorrect = i == 0 })] };
        return quiz with { Questions = [swapped, .. quiz.Questions.Skip(1)] };
    }
}
