using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.DTOs;

/// <summary>Desafio do dia do ponto de vista do usuário: progresso e meta vêm prontos do backend.</summary>
public record DailyChallengeDto(
    int Id,
    string Title,
    string Description,
    ChallengeType Type,
    int ExperienceReward,
    int Progress,
    int Target,
    bool IsCompleted,
    DateTimeOffset? CompletedAt);

/// <summary>Desafios de hoje (UTC). ResetsAt é a meia-noite UTC seguinte, quando novos desafios aparecem.</summary>
public record DailyChallengesDto(DateOnly Date, DateTimeOffset ResetsAt, IReadOnlyList<DailyChallengeDto> Challenges);

/// <summary>Item do histórico de desafios concluídos.</summary>
public record CompletedChallengeDto(
    int Id,
    string Title,
    string Description,
    int ExperienceReward,
    DateOnly Date,
    DateTimeOffset CompletedAt);
