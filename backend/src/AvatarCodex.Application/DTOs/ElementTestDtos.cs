namespace AvatarCodex.Application.DTOs;

/// <summary>Opção sem o elemento: o cliente não sabe qual resposta leva a qual elemento.</summary>
public record ElementTestOptionDto(int Id, string Text);

public record ElementTestQuestionDto(int Id, string Text, IReadOnlyList<ElementTestOptionDto> Options);

public record ElementTestDto(IReadOnlyList<ElementTestQuestionDto> Questions, int ExperienceReward);

public record ElementTestAnswerRequest(int QuestionId, int OptionId);

public record SubmitElementTestRequest(IReadOnlyList<ElementTestAnswerRequest>? Answers);

public record ElementScoreDto(ElementSummaryDto Element, int Points);

/// <summary>
/// Resultado: o elemento (com descrição), os pontos de cada um e o XP. ExperienceEarned é só o do teste
/// (30 na primeira vez, 0 ao refazer); Reward soma também conquistas e desafios do mesmo evento.
/// </summary>
public record ElementTestResultDto(
    ElementDto Element, IReadOnlyList<ElementScoreDto> Scores, int ExperienceEarned, RewardResult? Reward);
