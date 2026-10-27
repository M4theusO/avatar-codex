using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Domain.Entities;

/// <summary>Modelo de desafio (conteúdo do seed). Code é a chave estável usada pelo seed.</summary>
public class ChallengeTemplate
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public ChallengeType ChallengeType { get; set; }

    /// <summary>Quantidade, ou porcentagem em QuizScoreAtLeast.</summary>
    public int TargetValue { get; set; }

    public int ExperienceReward { get; set; }

    /// <summary>Inativo: não é mais sorteado, mas os desafios já gerados continuam valendo.</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>Um modelo sorteado para um dia (UTC). UNIQUE (ChallengeTemplateId, Date).</summary>
public class DailyChallenge
{
    public int Id { get; set; }
    public int ChallengeTemplateId { get; set; }
    public ChallengeTemplate Template { get; set; } = null!;
    public DateOnly Date { get; set; }
}

/// <summary>
/// Conclusão de um desafio pelo usuário. O progresso é calculado a partir das atividades do dia, então só a
/// conclusão é gravada; a PK (UserId, DailyChallengeId) impede concluir (e ganhar XP) duas vezes (RN-05).
/// </summary>
public class UserDailyChallenge
{
    public Guid UserId { get; set; }
    public int DailyChallengeId { get; set; }
    public DailyChallenge DailyChallenge { get; set; } = null!;
    public DateTimeOffset CompletedAt { get; set; }
}
