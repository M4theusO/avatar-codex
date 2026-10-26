using AvatarCodex.Domain.ValueObjects;

namespace AvatarCodex.Domain.Entities;

public class Nation
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public ImageInfo? SymbolImage { get; set; }

    /// <summary>Nulo para nações sem elemento próprio, como a República Unida.</summary>
    public int? ElementId { get; set; }
    public Element? Element { get; set; }

    public ICollection<Character> Characters { get; set; } = [];
}
