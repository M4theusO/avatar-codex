using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace AvatarCodex.Application.Services;

/// <summary>Quizzes: catálogo, jogo sem as respostas (RN-09), correção e recompensas (RN-06).</summary>
public class QuizService(
    IQuizRepository quizzes,
    ProgressionService progression,
    TimeProvider timeProvider,
    IValidator<QuizQueryParameters> queryValidator,
    IValidator<PageQueryParameters> pageValidator)
{
    public async Task<IReadOnlyList<QuizListItemDto>> GetAllAsync(
        QuizQueryParameters query, Guid? userId, CancellationToken cancellationToken = default)
    {
        await queryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var difficulty = query.Difficulty is null ? (QuizDifficulty?)null : Enum.Parse<QuizDifficulty>(query.Difficulty, ignoreCase: true);
        var category = string.IsNullOrWhiteSpace(query.Category) ? null : query.Category.Trim();
        var items = await quizzes.GetAllAsync(category, difficulty, cancellationToken);

        var best = userId is null
            ? new Dictionary<int, QuizBestResultDto>()
            : BestResults(await quizzes.GetAttemptScoresAsync(userId.Value, cancellationToken));

        return [.. items.Select(item => new QuizListItemDto(
            item.Quiz.Id,
            item.Quiz.Title,
            item.Quiz.Description,
            item.Quiz.Category,
            item.Quiz.Difficulty,
            item.Quiz.Image.ToDto(),
            item.QuestionCount,
            best.GetValueOrDefault(item.Quiz.Id)))];
    }

    /// <summary>Perguntas e opções para jogar: sem IsCorrect e sem explicação.</summary>
    public async Task<QuizDto> GetForPlayAsync(int id, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetWithQuestionsAsync(id, cancellationToken)
            ?? throw NotFoundException.For("Quiz", id);

        return new QuizDto(
            quiz.Id,
            quiz.Title,
            quiz.Description,
            quiz.Category,
            quiz.Difficulty,
            quiz.Image.ToDto(),
            [.. quiz.Questions.Select(q => new QuizQuestionDto(
                q.Id,
                q.Text,
                q.ImageUrl,
                [.. q.Options.Select(o => new QuizOptionDto(o.Id, o.Text))]))]);
    }

    /// <summary>
    /// Corrige no backend, grava a tentativa (sempre) e concede o XP: +20 na primeira conclusão do quiz e
    /// +50 na primeira vez em que o usuário gabarita (RN-06). Tentativas seguintes ficam só no histórico.
    /// </summary>
    public async Task<QuizAttemptResultDto> SubmitAsync(
        Guid userId, int quizId, SubmitQuizAttemptRequest request, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetWithQuestionsAsync(quizId, cancellationToken)
            ?? throw NotFoundException.For("Quiz", quizId);

        var selected = ValidateAnswers(quiz, request.Answers);
        var results = quiz.Questions
            .Select(question =>
            {
                var correct = question.Options.Single(o => o.IsCorrect);
                var optionId = selected[question.Id];
                return new QuestionResultDto(question.Id, optionId, correct.Id, optionId == correct.Id, question.Explanation);
            })
            .ToList();

        var score = results.Count(r => r.IsCorrect);
        var attempt = new UserQuizAttempt
        {
            UserId = userId,
            QuizId = quiz.Id,
            Score = score,
            TotalQuestions = results.Count,
            CompletedAt = timeProvider.GetUtcNow(),
            Answers = [.. results.Select(r => new UserQuizAnswer
            {
                QuestionId = r.QuestionId,
                SelectedOptionId = r.SelectedOptionId,
                IsCorrect = r.IsCorrect
            })]
        };
        // Gravada antes do XP: as conquistas de quiz contam as respostas desta tentativa.
        await quizzes.AddAttemptAsync(attempt, cancellationToken);

        var grants = new List<ExperienceGrant> { new(ExperienceType.QuizCompleted, quiz.Id, $"Concluiu o quiz {quiz.Title}") };
        if (score == results.Count)
            grants.Add(new ExperienceGrant(ExperienceType.QuizPerfect, quiz.Id, $"Gabaritou o quiz {quiz.Title}"));

        var reward = await progression.GrantAllAsync(userId, grants, cancellationToken);
        // O reward soma tudo o que o evento rendeu; o histórico do quiz guarda só o XP do próprio quiz.
        var quizXp = reward is null
            ? 0
            : reward.XpGained
              - reward.AchievementsUnlocked.Sum(a => a.ExperienceReward)
              - reward.ChallengesCompleted.Sum(c => c.ExperienceReward);
        if (quizXp > 0)
            await quizzes.SetExperienceEarnedAsync(attempt.Id, quizXp, cancellationToken);

        return new QuizAttemptResultDto(
            attempt.Id, score, results.Count, QuizScoring.Percentage(score, results.Count), quizXp, results, reward);
    }

    public async Task<PagedResult<QuizAttemptDto>> GetHistoryAsync(
        Guid userId, PageQueryParameters query, CancellationToken cancellationToken = default)
    {
        await pageValidator.ValidateAndThrowAsync(query, cancellationToken);

        var (items, totalItems) = await quizzes.GetHistoryAsync(userId, query.Page, query.PageSize, cancellationToken);

        return new PagedResult<QuizAttemptDto>(
            [.. items.Select(a => new QuizAttemptDto(
                a.Id,
                a.QuizId,
                a.Quiz.Title,
                a.Score,
                a.TotalQuestions,
                QuizScoring.Percentage(a.Score, a.TotalQuestions),
                a.ExperienceEarned,
                a.CompletedAt))],
            query.Page,
            query.PageSize,
            totalItems);
    }

    /// <summary>
    /// Todas as perguntas respondidas uma única vez, e cada opção pertencente à sua pergunta (docs/api.md).
    /// Retorna questionId → optionId.
    /// </summary>
    private static Dictionary<int, int> ValidateAnswers(Quiz quiz, IReadOnlyList<QuizAnswerRequest>? answers)
    {
        var errors = new List<ValidationFailure>();
        answers ??= [];
        var questions = quiz.Questions.ToDictionary(q => q.Id);

        foreach (var duplicated in answers.GroupBy(a => a.QuestionId).Where(g => g.Count() > 1))
            errors.Add(new ValidationFailure("answers", $"A pergunta {duplicated.Key} foi respondida mais de uma vez."));

        foreach (var answer in answers.DistinctBy(a => a.QuestionId))
        {
            if (!questions.TryGetValue(answer.QuestionId, out var question))
                errors.Add(new ValidationFailure("answers", $"A pergunta {answer.QuestionId} não pertence a este quiz."));
            else if (question.Options.All(o => o.Id != answer.OptionId))
                errors.Add(new ValidationFailure("answers", $"A opção {answer.OptionId} não pertence à pergunta {answer.QuestionId}."));
        }

        var answered = answers.Select(a => a.QuestionId).ToHashSet();
        var missing = quiz.Questions.Count(q => !answered.Contains(q.Id));
        if (missing > 0)
            errors.Add(new ValidationFailure("answers", missing == 1 ? "Falta responder 1 pergunta." : $"Faltam responder {missing} perguntas."));

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return answers.ToDictionary(a => a.QuestionId, a => a.OptionId);
    }

    /// <summary>Melhor porcentagem por quiz (o total pode mudar se o conteúdo do quiz mudar).</summary>
    private static Dictionary<int, QuizBestResultDto> BestResults(IReadOnlyList<AttemptScore> attempts) =>
        attempts
            .GroupBy(a => a.QuizId)
            .ToDictionary(g => g.Key, g =>
            {
                var best = g.MaxBy(a => QuizScoring.Percentage(a.Score, a.TotalQuestions))!;
                return new QuizBestResultDto(
                    best.Score, best.TotalQuestions, QuizScoring.Percentage(best.Score, best.TotalQuestions), g.Count());
            });
}

public static class QuizScoring
{
    /// <summary>Porcentagem de acertos arredondada (2 de 3 → 67).</summary>
    public static int Percentage(int score, int total) =>
        total == 0 ? 0 : (int)Math.Round(score * 100m / total, MidpointRounding.AwayFromZero);
}
