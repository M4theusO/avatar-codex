namespace AvatarCodex.Application.Services;

/// <summary>
/// Nível derivado do XP (RN-07): XP mínimo do nível n = 25 × (n − 1) × (n + 2).
/// Único lugar com a fórmula, para que ela possa mudar sem afetar o resto.
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
}
