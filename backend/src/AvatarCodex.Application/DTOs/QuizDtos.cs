using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.DTOs;

/// <summary>Filtros de GET /api/quizzes. Difficulty: Easy | Medium | Hard.</summary>
public class QuizQueryParameters
{
    public string? Category { get; set; }
    public string? Difficulty { get; set; }
}

/// <summary>Melhor tentativa do usuário naquele quiz.</summary>
public record QuizBestResultDto(int Score, int TotalQuestions, int Percentage, int Attempts);

public record QuizListItemDto(
    int Id,
    string Title,
    string Description,
    string Category,
    QuizDifficulty Difficulty,
    ImageDto? Image,
    int QuestionCount,
    QuizBestResultDto? BestResult);

/// <summary>Opção sem IsCorrect: este DTO é o único formato de opção antes da submissão (RN-09).</summary>
public record QuizOptionDto(int Id, string Text);

/// <summary>Pergunta sem a explicação, que também só aparece no resultado.</summary>
public record QuizQuestionDto(int Id, string Text, string? ImageUrl, IReadOnlyList<QuizOptionDto> Options);

public record QuizDto(
    int Id,
    string Title,
    string Description,
    string Category,
    QuizDifficulty Difficulty,
    ImageDto? Image,
    IReadOnlyList<QuizQuestionDto> Questions);

public record QuizAnswerRequest(int QuestionId, int OptionId);

public record SubmitQuizAttemptRequest(IReadOnlyList<QuizAnswerRequest>? Answers);

public record QuestionResultDto(
    int QuestionId,
    int SelectedOptionId,
    int CorrectOptionId,
    bool IsCorrect,
    string? Explanation);

/// <summary>
/// Resultado da submissão. Reward é null quando a tentativa não rendeu XP nem conquista
/// (quiz refeito sem gabaritar pela primeira vez).
/// </summary>
public record QuizAttemptResultDto(
    int AttemptId,
    int Score,
    int TotalQuestions,
    int Percentage,
    int ExperienceEarned,
    IReadOnlyList<QuestionResultDto> Results,
    RewardResult? Reward);

/// <summary>Item do histórico de quizzes.</summary>
public record QuizAttemptDto(
    int Id,
    int QuizId,
    string QuizTitle,
    int Score,
    int TotalQuestions,
    int Percentage,
    int ExperienceEarned,
    DateTimeOffset CompletedAt);
