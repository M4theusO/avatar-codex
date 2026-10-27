namespace AvatarCodex.Domain.Enums;

/// <summary>O que um desafio diário mede, sempre dentro do dia (UTC) do desafio.</summary>
public enum ChallengeType
{
    /// <summary>Personagens descobertos no dia.</summary>
    DiscoverCharacters,

    /// <summary>Nações descobertas no dia.</summary>
    DiscoverNations,

    /// <summary>Quizzes diferentes concluídos no dia (refazer um quiz já concluído antes também conta).</summary>
    CompleteQuizzes,

    /// <summary>Melhor porcentagem de acertos numa tentativa do dia; TargetValue é a porcentagem.</summary>
    QuizScoreAtLeast
}
