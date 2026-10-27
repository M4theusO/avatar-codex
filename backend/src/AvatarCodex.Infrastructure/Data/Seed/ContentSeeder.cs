using AvatarCodex.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Data.Seed;

/// <summary>
/// Aplica o conteúdo de demonstração de forma idempotente: insere o que falta e atualiza o que mudou,
/// usando o nome (ou código) como chave natural. Nunca remove conteúdo que não esteja no seed.
/// </summary>
public class ContentSeeder(AppDbContext db)
{
    public async Task SeedAsync(ContentSeedData data, CancellationToken cancellationToken = default)
    {
        var errors = data.Validate();
        if (errors.Count > 0)
            throw new InvalidOperationException("Seed de conteúdo inválido:\n" + string.Join("\n", errors));

        var elements = await db.Elements.ToDictionaryAsync(e => e.Name, cancellationToken);
        foreach (var seed in data.Elements)
        {
            var element = GetOrAdd(elements, seed.Name, () => new Element
            {
                Name = seed.Name, Description = seed.Description, Color = seed.Color, Icon = seed.Icon
            });
            element.Description = seed.Description;
            element.Color = seed.Color;
            element.Icon = seed.Icon;
        }

        var subtypes = await db.BendingSubtypes.ToDictionaryAsync(b => b.Name, cancellationToken);
        foreach (var seed in data.BendingSubtypes)
        {
            var subtype = GetOrAdd(subtypes, seed.Name, () => new BendingSubtype
            {
                Name = seed.Name, Description = seed.Description
            });
            subtype.Description = seed.Description;
            subtype.Element = elements[seed.Element];
        }

        var nations = await db.Nations.ToDictionaryAsync(n => n.Name, cancellationToken);
        foreach (var seed in data.Nations)
        {
            var nation = GetOrAdd(nations, seed.Name, () => new Nation
            {
                Name = seed.Name, Description = seed.Description
            });
            nation.Description = seed.Description;
            nation.Element = Lookup(elements, seed.Element);
        }

        var series = await db.Series.ToDictionaryAsync(s => s.Code, cancellationToken);
        foreach (var seed in data.Series)
        {
            var item = GetOrAdd(series, seed.Code, () => new Series { Code = seed.Code, Name = seed.Name });
            item.Name = seed.Name;
        }

        var abilities = await db.Abilities.ToDictionaryAsync(a => a.Name, cancellationToken);
        foreach (var seed in data.Abilities)
        {
            var ability = GetOrAdd(abilities, seed.Name, () => new Ability
            {
                Name = seed.Name, Description = seed.Description
            });
            ability.Description = seed.Description;
            ability.Element = Lookup(elements, seed.Element);
        }

        var characters = await db.Characters
            .Include(c => c.BendingSubtypes)
            .Include(c => c.Abilities)
            .Include(c => c.Series)
            .Include(c => c.Relationships)
            .AsSplitQuery()
            .ToDictionaryAsync(c => c.Name, cancellationToken);

        // Primeiro garante que todos os personagens existem, para que as relações possam referenciá-los.
        foreach (var seed in data.Characters)
        {
            var character = GetOrAdd(characters, seed.Name, () => new Character
            {
                Name = seed.Name, Description = seed.Description, Biography = seed.Biography
            });
            if (!character.Aliases.SequenceEqual(seed.Aliases))
                character.Aliases = [.. seed.Aliases];
            character.Description = seed.Description;
            character.Biography = seed.Biography;
            character.Nation = Lookup(nations, seed.Nation);
            character.Element = Lookup(elements, seed.Element);
            character.IsBender = seed.IsBender;
            character.IsAvatar = seed.IsAvatar;
            character.Status = seed.Status;
            character.FirstAppearance = seed.FirstAppearance;

            Sync(character.Series, seed.Series.Select(code => series[code]));
            Sync(character.BendingSubtypes, seed.BendingSubtypes.Select(name => subtypes[name]));
            Sync(character.Abilities, seed.Abilities.Select(name => abilities[name]));
        }

        foreach (var seed in data.Characters)
            SyncRelationships(characters[seed.Name], seed.Relationships, characters);

        var achievements = await db.Achievements.ToDictionaryAsync(a => a.Code, cancellationToken);
        foreach (var seed in data.Achievements)
        {
            var achievement = GetOrAdd(achievements, seed.Code, () => new Achievement
            {
                Code = seed.Code, Name = seed.Name, Description = seed.Description, Icon = seed.Icon
            });
            achievement.Name = seed.Name;
            achievement.Description = seed.Description;
            achievement.Icon = seed.Icon;
            achievement.RequirementType = seed.RequirementType;
            achievement.RequirementValue = seed.RequirementValue;
            achievement.ExperienceReward = seed.ExperienceReward;
        }

        var challenges = await db.ChallengeTemplates.ToDictionaryAsync(c => c.Code, cancellationToken);
        foreach (var seed in data.Challenges)
        {
            var challenge = GetOrAdd(challenges, seed.Code, () => new ChallengeTemplate
            {
                Code = seed.Code, Title = seed.Title, Description = seed.Description
            });
            challenge.Title = seed.Title;
            challenge.Description = seed.Description;
            challenge.ChallengeType = seed.ChallengeType;
            challenge.TargetValue = seed.TargetValue;
            challenge.ExperienceReward = seed.ExperienceReward;
            challenge.IsActive = seed.IsActive;
        }

        await db.SaveChangesAsync(cancellationToken);
        await SeedQuizzesAsync(data.Quizzes, cancellationToken);
    }

    /// <summary>
    /// Perguntas e opções são sincronizadas pela posição (Order). Nunca são removidas: respostas de
    /// tentativas antigas apontam para elas. A opção correta é marcada numa segunda gravação, para a
    /// troca de resposta não violar o índice de "uma correta por pergunta" no meio da primeira.
    /// </summary>
    private async Task SeedQuizzesAsync(List<QuizSeed> seeds, CancellationToken cancellationToken)
    {
        var quizzes = await db.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Options)
            .AsSplitQuery()
            .ToDictionaryAsync(q => q.Title, cancellationToken);

        var correctOptions = new List<QuestionOption>();
        foreach (var seed in seeds)
        {
            var quiz = GetOrAdd(quizzes, seed.Title, () => new Quiz
            {
                Title = seed.Title, Description = seed.Description, Category = seed.Category
            });
            quiz.Description = seed.Description;
            quiz.Category = seed.Category;
            quiz.Difficulty = seed.Difficulty;

            foreach (var (questionSeed, order) in seed.Questions.Select((q, i) => (q, i + 1)))
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Order == order);
                if (question is null)
                {
                    question = new Question { Text = questionSeed.Text, Order = order };
                    quiz.Questions.Add(question);
                }
                question.Text = questionSeed.Text;
                question.Explanation = questionSeed.Explanation;

                foreach (var (optionSeed, optionOrder) in questionSeed.Options.Select((o, i) => (o, i + 1)))
                {
                    var option = question.Options.FirstOrDefault(o => o.Order == optionOrder);
                    if (option is null)
                    {
                        option = new QuestionOption { Text = optionSeed.Text, Order = optionOrder };
                        question.Options.Add(option);
                    }
                    option.Text = optionSeed.Text;
                    if (!optionSeed.IsCorrect)
                        option.IsCorrect = false;
                    else if (!option.IsCorrect)
                        correctOptions.Add(option);
                }

                // Opções que saíram do seed ficam no banco (tentativas antigas apontam para elas), mas nunca corretas.
                foreach (var stale in question.Options.Where(o => o.Order > questionSeed.Options.Count))
                    stale.IsCorrect = false;
            }
        }
        await db.SaveChangesAsync(cancellationToken);

        foreach (var option in correctOptions)
            option.IsCorrect = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    private TEntity GetOrAdd<TEntity>(Dictionary<string, TEntity> existing, string key, Func<TEntity> create)
        where TEntity : class
    {
        if (existing.TryGetValue(key, out var entity))
            return entity;

        entity = create();
        db.Add(entity);
        existing[key] = entity;
        return entity;
    }

    private static TEntity? Lookup<TEntity>(Dictionary<string, TEntity> items, string? key) where TEntity : class =>
        key is null ? null : items[key];

    private static void Sync<TEntity>(ICollection<TEntity> current, IEnumerable<TEntity> desired) where TEntity : class
    {
        var desiredList = desired.ToList();
        foreach (var item in current.Where(c => !desiredList.Contains(c)).ToList())
            current.Remove(item);
        foreach (var item in desiredList.Where(d => !current.Contains(d)))
            current.Add(item);
    }

    private static void SyncRelationships(
        Character character, List<RelationshipSeed> desired, Dictionary<string, Character> characters)
    {
        foreach (var existing in character.Relationships.ToList())
        {
            if (!desired.Any(d => characters[d.Related] == existing.RelatedCharacter && d.Type == existing.Type))
                character.Relationships.Remove(existing);
        }

        foreach (var seed in desired)
        {
            var related = characters[seed.Related];
            var relationship = character.Relationships
                .FirstOrDefault(r => r.RelatedCharacter == related && r.Type == seed.Type);

            if (relationship is null)
            {
                relationship = new CharacterRelationship { RelatedCharacter = related, Type = seed.Type };
                character.Relationships.Add(relationship);
            }
            relationship.Description = seed.Description;
        }
    }
}
