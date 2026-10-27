using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Services;
using AvatarCodex.Application.Validators;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Exceptions;
using AvatarCodex.UnitTests.Fakes;
using FluentValidation;

namespace AvatarCodex.UnitTests.Quizzes;

public class QuizServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly FakeQuizRepository _quizzes = new();
    private readonly FakeProgressionRepository _progression = new();
    private readonly FakeAchievementRepository _achievements;
    private readonly QuizService _service;

    public QuizServiceTests()
    {
        _achievements = new FakeAchievementRepository(_progression);
        _service = new QuizService(
            _quizzes,
            new ProgressionService(_progression, _achievements, TimeProvider.System),
            TimeProvider.System,
            new QuizQueryParametersValidator(),
            new PageQueryParametersValidator());
    }

    // Quiz 1: perguntas 10 e 11; as opções corretas são 101 e 112.
    private static readonly SubmitQuizAttemptRequest Perfect = Answers((10, 101), (11, 112));
    private static readonly SubmitQuizAttemptRequest HalfRight = Answers((10, 101), (11, 111));

    private static SubmitQuizAttemptRequest Answers(params (int Question, int Option)[] answers) =>
        new([.. answers.Select(a => new QuizAnswerRequest(a.Question, a.Option))]);

    [Fact]
    public async Task GetForPlay_NeverExposesCorrectAnswersOrExplanations()
    {
        var quiz = await _service.GetForPlayAsync(1);

        Assert.Equal([10, 11], quiz.Questions.Select(q => q.Id));
        Assert.Equal([101, 102], quiz.Questions[0].Options.Select(o => o.Id));
        // RN-09: o formato de opção para jogar não tem como carregar a resposta.
        Assert.Equal(["Id", "Text"], typeof(QuizOptionDto).GetProperties().Select(p => p.Name));
        Assert.DoesNotContain(typeof(QuizQuestionDto).GetProperties(), p => p.Name == "Explanation");
    }

    [Fact]
    public async Task GetForPlay_UnknownQuiz_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetForPlayAsync(999));
    }

    [Fact]
    public async Task Submit_CorrectsInTheBackendAndReturnsCorrectionPerQuestion()
    {
        var result = await _service.SubmitAsync(UserId, 1, HalfRight);

        Assert.Equal((1, 2, 50), (result.Score, result.TotalQuestions, result.Percentage));
        Assert.Equal(new QuestionResultDto(10, 101, 101, true, "Explicação 1"), result.Results[0]);
        Assert.Equal(new QuestionResultDto(11, 111, 112, false, "Explicação 2"), result.Results[1]);

        var attempt = Assert.Single(_quizzes.Attempts);
        Assert.Equal((1, 2), (attempt.Score, attempt.TotalQuestions));
        Assert.Equal([true, false], attempt.Answers.Select(a => a.IsCorrect));
    }

    [Fact]
    public async Task Submit_FirstCompletion_GrantsQuizXp()
    {
        var result = await _service.SubmitAsync(UserId, 1, HalfRight);

        Assert.Equal(20, result.ExperienceEarned);
        Assert.Equal(20, result.Reward!.XpGained);
        Assert.Equal(20, _quizzes.Attempts.Single().ExperienceEarned);
        Assert.Equal(ExperienceType.QuizCompleted, _progression.Transactions.Single().Type);
    }

    [Fact]
    public async Task Submit_FirstPerfect_GrantsCompletionAndPerfectBonus()
    {
        var result = await _service.SubmitAsync(UserId, 1, Perfect);

        Assert.Equal(70, result.ExperienceEarned);
        Assert.Equal(
            [ExperienceType.QuizCompleted, ExperienceType.QuizPerfect],
            _progression.Transactions.Select(t => t.Type));
    }

    [Fact]
    public async Task Submit_Again_IsRecordedWithoutXp()
    {
        await _service.SubmitAsync(UserId, 1, Perfect);

        var again = await _service.SubmitAsync(UserId, 1, Perfect);

        // RN-06: refazer o quiz fica no histórico, mas sem XP.
        Assert.Equal(0, again.ExperienceEarned);
        Assert.Null(again.Reward);
        Assert.Equal(2, _quizzes.Attempts.Count);
        Assert.Equal(70, _progression.TotalXp[UserId]);
    }

    [Fact]
    public async Task Submit_FirstPerfectAfterAnEarlierAttempt_GrantsOnlyTheBonus()
    {
        await _service.SubmitAsync(UserId, 1, HalfRight);

        var perfect = await _service.SubmitAsync(UserId, 1, Perfect);

        Assert.Equal(50, perfect.ExperienceEarned);
        Assert.Equal(70, _progression.TotalXp[UserId]);
    }

    [Fact]
    public async Task Submit_WithoutXp_StillEvaluatesAchievements()
    {
        _achievements.Catalog.Add(new Achievement
        {
            Id = 7, Code = "SCHOLAR", Name = "Conhecedor", Description = "", Icon = "brain",
            RequirementType = AchievementRequirementType.CorrectAnswers, RequirementValue = 3, ExperienceReward = 150
        });
        await _service.SubmitAsync(UserId, 1, Perfect);
        _achievements.CorrectAnswers = 3; // o repositório real conta as respostas gravadas

        var again = await _service.SubmitAsync(UserId, 1, HalfRight);

        Assert.Equal(0, again.ExperienceEarned);
        Assert.Equal("Conhecedor", Assert.Single(again.Reward!.AchievementsUnlocked).Name);
        Assert.Equal(150, again.Reward.XpGained);
    }

    [Theory]
    [MemberData(nameof(InvalidSubmissions))]
    public async Task Submit_InvalidAnswers_ThrowsValidationWithoutRecording(SubmitQuizAttemptRequest request, string message)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.SubmitAsync(UserId, 1, request));

        Assert.Contains(ex.Errors, e => e.PropertyName == "answers" && e.ErrorMessage.Contains(message));
        Assert.Empty(_quizzes.Attempts);
        Assert.Empty(_progression.Transactions);
    }

    public static TheoryData<SubmitQuizAttemptRequest, string> InvalidSubmissions() => new()
    {
        { new SubmitQuizAttemptRequest(null), "Faltam responder 2 perguntas" },
        { Answers((10, 101)), "Falta responder 1 pergunta" },
        { Answers((10, 101), (10, 102), (11, 112)), "respondida mais de uma vez" },
        { Answers((10, 112), (11, 112)), "A opção 112 não pertence à pergunta 10" },
        { Answers((10, 101), (11, 112), (20, 201)), "A pergunta 20 não pertence a este quiz" }
    };

    [Fact]
    public async Task Submit_UnknownQuiz_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _service.SubmitAsync(UserId, 999, Perfect));
    }

    [Fact]
    public async Task GetAll_ShowsTheBestResultOnlyForTheSignedUser()
    {
        await _service.SubmitAsync(UserId, 1, HalfRight);
        await _service.SubmitAsync(UserId, 1, Perfect);
        await _service.SubmitAsync(UserId, 1, HalfRight);

        var mine = await _service.GetAllAsync(new QuizQueryParameters(), UserId);
        var visitor = await _service.GetAllAsync(new QuizQueryParameters(), null);

        Assert.Equal(new QuizBestResultDto(2, 2, 100, 3), mine.Single(q => q.Id == 1).BestResult);
        Assert.Null(mine.Single(q => q.Id == 2).BestResult);
        Assert.All(visitor, q => Assert.Null(q.BestResult));
        Assert.Equal(2, mine.Single(q => q.Id == 1).QuestionCount);
    }

    [Fact]
    public async Task GetAll_InvalidDifficulty_ThrowsValidation()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetAllAsync(new QuizQueryParameters { Difficulty = "impossible" }, null));
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.GetAllAsync(new QuizQueryParameters { Difficulty = "1" }, null));
    }

    [Fact]
    public async Task GetAll_FiltersByDifficultyIgnoringCase()
    {
        var hard = await _service.GetAllAsync(new QuizQueryParameters { Difficulty = "hard" }, null);

        Assert.Equal(["Avançado"], hard.Select(q => q.Title));
    }

    [Theory]
    [InlineData(2, 3, 67)]
    [InlineData(1, 3, 33)]
    [InlineData(1, 2, 50)]
    [InlineData(0, 0, 0)]
    public void Percentage_IsRounded(int score, int total, int expected)
    {
        Assert.Equal(expected, QuizScoring.Percentage(score, total));
    }

    /// <summary>Dois quizzes em memória: o 1 (fácil, 2 perguntas) e o 2 (difícil, 1 pergunta).</summary>
    private sealed class FakeQuizRepository : IQuizRepository
    {
        private readonly List<Quiz> _quizzes =
        [
            new()
            {
                Id = 1, Title = "Básico", Description = "", Category = "Personagens", Difficulty = QuizDifficulty.Easy,
                Questions =
                [
                    Question(10, 1, "Explicação 1", (101, true), (102, false)),
                    Question(11, 2, "Explicação 2", (111, false), (112, true))
                ]
            },
            new()
            {
                Id = 2, Title = "Avançado", Description = "", Category = "História", Difficulty = QuizDifficulty.Hard,
                Questions = [Question(20, 1, null, (201, true), (202, false))]
            }
        ];

        public List<UserQuizAttempt> Attempts { get; } = [];

        private static Question Question(int id, int order, string? explanation, params (int Id, bool IsCorrect)[] options) => new()
        {
            Id = id, Text = $"Pergunta {id}", Order = order, Explanation = explanation,
            Options = [.. options.Select((o, i) => new QuestionOption { Id = o.Id, Text = $"Opção {o.Id}", IsCorrect = o.IsCorrect, Order = i + 1 })]
        };

        public Task<IReadOnlyList<QuizSummary>> GetAllAsync(string? category, QuizDifficulty? difficulty, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<QuizSummary>>([.. _quizzes
                .Where(q => (category is null || q.Category == category) && (difficulty is null || q.Difficulty == difficulty))
                .Select(q => new QuizSummary(q, q.Questions.Count))]);

        public Task<Quiz?> GetWithQuestionsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(_quizzes.SingleOrDefault(q => q.Id == id));

        public Task<IReadOnlyList<AttemptScore>> GetAttemptScoresAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<AttemptScore>>([.. Attempts
                .Where(a => a.UserId == userId)
                .Select(a => new AttemptScore(a.QuizId, a.Score, a.TotalQuestions))]);

        public Task AddAttemptAsync(UserQuizAttempt attempt, CancellationToken cancellationToken)
        {
            attempt.Id = Attempts.Count + 1;
            Attempts.Add(attempt);
            return Task.CompletedTask;
        }

        public Task SetExperienceEarnedAsync(int attemptId, int experience, CancellationToken cancellationToken)
        {
            Attempts.Single(a => a.Id == attemptId).ExperienceEarned = experience;
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<UserQuizAttempt> Items, int TotalItems)> GetHistoryAsync(
            Guid userId, int page, int pageSize, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
