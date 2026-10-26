using AvatarCodex.Domain.ValueObjects;

namespace AvatarCodex.Domain.Entities;

public class Ability
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    /// <summary>Nulo para habilidades que não dependem de dobra (ex. bloqueio de chi).</summary>
    public int? ElementId { get; set; }
    public Element? Element { get; set; }

    public ImageInfo? Image { get; set; }

    public ICollection<Character> Characters { get; set; } = [];
}
