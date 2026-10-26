using AvatarCodex.Domain.Enums;

namespace AvatarCodex.Domain.Entities;

/// <summary>
/// Relação direcional: <see cref="RelatedCharacter"/> é "<see cref="Description"/>" de <see cref="Character"/>
/// (ex. Zuko → Iroh, Family, "Tio").
/// </summary>
public class CharacterRelationship
{
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public int RelatedCharacterId { get; set; }
    public Character RelatedCharacter { get; set; } = null!;

    public RelationshipType Type { get; set; }
    public string? Description { get; set; }
}
