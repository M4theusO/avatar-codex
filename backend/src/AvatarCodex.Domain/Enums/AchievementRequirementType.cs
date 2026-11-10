namespace AvatarCodex.Domain.Enums;

/// <summary>O que é medido para desbloquear uma conquista (comparado com Achievement.RequirementValue).</summary>
public enum AchievementRequirementType
{
    AccountCreated,
    CharactersDiscovered,
    NationsDiscovered,

    /// <summary>Todas as nações cadastradas: a meta acompanha o conteúdo, RequirementValue é ignorado.</summary>
    AllNationsDiscovered,

    AbilitiesDiscovered,

    /// <summary>Todas as habilidades cadastradas: a meta acompanha o conteúdo, RequirementValue é ignorado.</summary>
    AllAbilitiesDiscovered,

    QuizzesCompleted,
    PerfectQuizzes,
    CorrectAnswers,
    ChallengesCompleted,
    LevelReached
}
