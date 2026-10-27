using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.ValueObjects;

namespace AvatarCodex.Domain.Entities;

/// <summary>Quiz do catálogo (conteúdo do seed). Title é a chave natural usada pelo seed.</summary>
public class Quiz
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }

    /// <summary>Texto livre, definido no conteúdo (ex.: "Personagens").</summary>
    public required string Category { get; set; }

    public QuizDifficulty Difficulty { get; set; }
    public ImageInfo? Image { get; set; }

    public List<Question> Questions { get; set; } = [];
}

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public required string Text { get; set; }
    public string? ImageUrl { get; set; }

    /// <summary>Exibida só no resultado, depois da submissão.</summary>
    public string? Explanation { get; set; }

    public int Order { get; set; }
    public List<QuestionOption> Options { get; set; } = [];
}

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public required string Text { get; set; }

    /// <summary>Nunca sai da API antes da submissão (RN-09). Exatamente uma por pergunta.</summary>
    public bool IsCorrect { get; set; }

    public int Order { get; set; }
}

/// <summary>Tentativa de quiz. Toda tentativa é registrada; o XP só vem nas primeiras (RN-06).</summary>
public class UserQuizAttempt
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    /// <summary>Quantidade de acertos.</summary>
    public int Score { get; set; }

    public int TotalQuestions { get; set; }

    /// <summary>XP do quiz nesta tentativa (conclusão + perfeito), sem o XP das conquistas.</summary>
    public int ExperienceEarned { get; set; }

    public DateTimeOffset CompletedAt { get; set; }
    public List<UserQuizAnswer> Answers { get; set; } = [];
}

public class UserQuizAnswer
{
    public int UserQuizAttemptId { get; set; }
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }
}
