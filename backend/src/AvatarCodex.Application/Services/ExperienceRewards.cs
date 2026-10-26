using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Application.Services;

/// <summary>Tabela oficial de XP (docs/requirements.md §5.2). Único lugar com esses valores (RN-04).</summary>
public static class ExperienceRewards
{
    public const int CharacterDiscovered = 10;
    public const int NationDiscovered = 10;
    public const int QuizCompleted = 20;
    public const int QuizPerfect = 50;

    /// <summary>Valor fixo do tipo. Desafios e conquistas têm valor próprio, definido no conteúdo.</summary>
    public static int For(ExperienceType type) => type switch
    {
        ExperienceType.CharacterDiscovered => CharacterDiscovered,
        ExperienceType.NationDiscovered => NationDiscovered,
        ExperienceType.QuizCompleted => QuizCompleted,
        ExperienceType.QuizPerfect => QuizPerfect,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "O XP deste tipo vem do desafio ou da conquista.")
    };
}
