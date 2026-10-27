using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Interfaces;

public record QuizSummary(Quiz Quiz, int QuestionCount);

/// <summary>Score e total de uma tentativa, para calcular o melhor resultado por quiz.</summary>
public record AttemptScore(int QuizId, int Score, int TotalQuestions);

public interface IQuizRepository
{
    /// <summary>Quizzes com a quantidade de perguntas, sem carregar perguntas e opções.</summary>
    Task<IReadOnlyList<QuizSummary>> GetAllAsync(string? category, QuizDifficulty? difficulty, CancellationToken cancellationToken);

    /// <summary>Quiz com perguntas e opções, já na ordem de exibição.</summary>
    Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AttemptScore>> GetAttemptScoresAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Grava a tentativa com as respostas (sempre, mesmo sem XP).</summary>
    Task AddAttemptAsync(UserQuizAttempt attempt, CancellationToken cancellationToken);

    Task SetExperienceEarnedAsync(int attemptId, int experience, CancellationToken cancellationToken);

    /// <summary>Tentativas mais recentes primeiro, com o Quiz carregado.</summary>
    Task<(IReadOnlyList<UserQuizAttempt> Items, int TotalItems)> GetHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken);
}
