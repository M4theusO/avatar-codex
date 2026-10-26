namespace AvatarCodex.Domain.Entities;

public class Element
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    /// <summary>Cor de destaque em hexadecimal, ex. "#3186B5".</summary>
    public required string Color { get; set; }

    /// <summary>Nome do ícone usado pelo frontend.</summary>
    public required string Icon { get; set; }

    public ICollection<BendingSubtype> BendingSubtypes { get; set; } = [];
}
