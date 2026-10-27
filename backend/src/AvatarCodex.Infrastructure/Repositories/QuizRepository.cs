using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AvatarCodex.Infrastructure.Repositories;

public class QuizRepository(AppDbContext db) : IQuizRepository
{
    public async Task<IReadOnlyList<QuizSummary>> GetAllAsync(
        string? category, QuizDifficulty? difficulty, CancellationToken cancellationToken)
    {
        var query = db.Quizzes.AsNoTracking();
        if (category is not null)
            query = query.Where(q => q.Category.ToLower() == category.ToLower());
        if (difficulty is not null)
            query = query.Where(q => q.Difficulty == difficulty);

        var items = await query
            .Select(q => new { Quiz = q, QuestionCount = q.Questions.Count })
            .ToListAsync(cancellationToken);

        // Ordem por dificuldade (enum gravado como texto) e título: feita em memória, o catálogo é pequeno.
        return [.. items
            .OrderBy(i => i.Quiz.Difficulty)
            .ThenBy(i => i.Quiz.Title)
            .Select(i => new QuizSummary(i.Quiz, i.QuestionCount))];
    }

    public Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken cancellationToken) =>
        db.Quizzes
            .AsNoTracking()
            .Include(q => q.Questions.OrderBy(question => question.Order))
            .ThenInclude(question => question.Options.OrderBy(option => option.Order))
            .AsSplitQuery()
            .SingleOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AttemptScore>> GetAttemptScoresAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.QuizAttempts
            .Where(a => a.UserId == userId)
            .Select(a => new AttemptScore(a.QuizId, a.Score, a.TotalQuestions))
            .ToListAsync(cancellationToken);

    public async Task AddAttemptAsync(UserQuizAttempt attempt, CancellationToken cancellationToken)
    {
        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task SetExperienceEarnedAsync(int attemptId, int experience, CancellationToken cancellationToken) =>
        db.QuizAttempts
            .Where(a => a.Id == attemptId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.ExperienceEarned, experience), cancellationToken);

    public async Task<(IReadOnlyList<UserQuizAttempt> Items, int TotalItems)> GetHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var attempts = db.QuizAttempts.AsNoTracking().Where(a => a.UserId == userId);

        var totalItems = await attempts.CountAsync(cancellationToken);
        var items = await attempts
            .Include(a => a.Quiz)
            .OrderByDescending(a => a.CompletedAt)
            .ThenByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }
}
