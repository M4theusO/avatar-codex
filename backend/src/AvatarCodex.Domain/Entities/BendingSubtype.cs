namespace AvatarCodex.Domain.Entities;

/// <summary>Especialização de dobra (metal, sangue, lava, relâmpago…).</summary>
public class BendingSubtype
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    public int ElementId { get; set; }
    public Element Element { get; set; } = null!;

    public ICollection<Character> Characters { get; set; } = [];
}
