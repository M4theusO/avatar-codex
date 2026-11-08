using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;

namespace AvatarCodex.Application.Services;

/// <summary>Teste de elemento: perguntas do banco, pontuação no backend, resultado salvo no perfil.</summary>
public class ElementTestService(IElementTestRepository repository, ProgressionService progression)
{
    public async Task<ElementTestDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var questions = await repository.GetQuestionsAsync(cancellationToken);
        return new ElementTestDto(
            [.. questions.Select(q => new ElementTestQuestionDto(
                q.Id, q.Text, [.. q.Options.Select(o => new ElementTestOptionDto(o.Id, o.Text))]))],
            ExperienceRewards.ElementTestCompleted);
    }

    /// <summary>
    /// Calcula o elemento, grava em User.ElementId (refazer substitui o anterior) e concede +30 XP só na
    /// primeira conclusão. Respostas incompletas, repetidas ou de outra pergunta → 400.
    /// </summary>
    public async Task<ElementTestResultDto> SubmitAsync(
        Guid userId, SubmitElementTestRequest request, CancellationToken cancellationToken = default)
    {
        var questions = await repository.GetQuestionsAsync(cancellationToken);
        var selected = ValidateAnswers(questions, request.Answers);

        var elements = await repository.GetElementsAsync(cancellationToken);
        var (winnerId, scores) = ElementTestRules.Score(questions, selected, [.. elements.Select(e => e.Id)]);
        var winner = elements.Single(e => e.Id == winnerId);

        await repository.SetUserElementAsync(userId, winnerId, cancellationToken);
        var reward = await progression.GrantAllAsync(userId,
            [new ExperienceGrant(ExperienceType.ElementTestCompleted, ElementTestRules.ExperienceSourceId,
                $"Descobriu seu elemento: {winner.Name}")],
            cancellationToken);

        var testXp = reward is null
            ? 0
            : reward.XpGained
              - reward.AchievementsUnlocked.Sum(a => a.ExperienceReward)
              - reward.ChallengesCompleted.Sum(c => c.ExperienceReward);

        return new ElementTestResultDto(
            winner.ToDto(),
            [.. scores.Select(s => new ElementScoreDto(elements.Single(e => e.Id == s.ElementId).ToSummaryDto()!, s.Points))],
            testXp,
            reward);
    }

    private static Dictionary<int, int> ValidateAnswers(
        IReadOnlyList<ElementTestQuestion> questions, IReadOnlyList<ElementTestAnswerRequest>? answers)
    {
        var errors = new List<ValidationFailure>();
        answers ??= [];
        var byId = questions.ToDictionary(q => q.Id);

        foreach (var duplicated in answers.GroupBy(a => a.QuestionId).Where(g => g.Count() > 1))
            errors.Add(new ValidationFailure("answers", $"A pergunta {duplicated.Key} foi respondida mais de uma vez."));

        foreach (var answer in answers.DistinctBy(a => a.QuestionId))
        {
            if (!byId.TryGetValue(answer.QuestionId, out var question))
                errors.Add(new ValidationFailure("answers", $"A pergunta {answer.QuestionId} não pertence ao teste."));
            else if (question.Options.All(o => o.Id != answer.OptionId))
                errors.Add(new ValidationFailure("answers", $"A opção {answer.OptionId} não pertence à pergunta {answer.QuestionId}."));
        }

        var missing = questions.Count(q => answers.All(a => a.QuestionId != q.Id));
        if (missing > 0)
            errors.Add(new ValidationFailure("answers",
                missing == 1 ? "Falta responder 1 pergunta." : $"Faltam responder {missing} perguntas."));

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return answers.ToDictionary(a => a.QuestionId, a => a.OptionId);
    }
}
