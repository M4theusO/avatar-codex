namespace AvatarCodex.Application.Services;

/// <summary>Posição do usuário dentro do nível atual.</summary>
/// <param name="CurrentLevelXp">XP mínimo do nível atual.</param>
/// <param name="NextLevelXp">XP mínimo do próximo nível.</param>
/// <param name="ProgressPercent">0–99: quanto do caminho até o próximo nível já foi feito.</param>
public record LevelProgress(int Level, int CurrentLevelXp, int NextLevelXp, int ProgressPercent);

/// <summary>
/// Nível derivado do XP (RN-07): XP mínimo do nível n = 25 × (n − 1) × (n + 2).
/// Único lugar com a fórmula, para que ela possa mudar sem afetar o resto
/// (o frontend recebe tudo pronto, inclusive a porcentagem da barra).
/// </summary>
public static class LevelCalculator
{
    public static int MinimumXpFor(int level) =>
        level <= 1 ? 0 : 25 * (level - 1) * (level + 2);

    public static int GetLevel(int experiencePoints)
    {
        var level = 1;
        while (experiencePoints >= MinimumXpFor(level + 1))
            level++;
        return level;
    }

    public static LevelProgress GetProgress(int experiencePoints)
    {
        var level = GetLevel(experiencePoints);
        var current = MinimumXpFor(level);
        var next = MinimumXpFor(level + 1);
        var percent = (int)((long)(experiencePoints - current) * 100 / (next - current));
        return new LevelProgress(level, current, next, percent);
    }
}
