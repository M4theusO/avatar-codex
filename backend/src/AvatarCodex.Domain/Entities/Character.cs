using AvatarCodex.Domain.Enums;
using AvatarCodex.Domain.Interfaces;
using AvatarCodex.Domain.ValueObjects;

namespace AvatarCodex.Domain.Entities;

public class Character : IAuditableEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public List<string> Aliases { get; set; } = [];

    /// <summary>Resumo curto, usado nos cards.</summary>
    public required string Description { get; set; }
    public required string Biography { get; set; }
    public ImageInfo? Image { get; set; }

    public int? NationId { get; set; }
    public Nation? Nation { get; set; }

    /// <summary>Elemento principal/nativo. Nulo para não-dobradores.</summary>
    public int? ElementId { get; set; }
    public Element? Element { get; set; }

    public bool IsBender { get; set; }
    public bool IsAvatar { get; set; }
    public CharacterStatus Status { get; set; }
    public string? FirstAppearance { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<BendingSubtype> BendingSubtypes { get; set; } = [];
    public ICollection<Ability> Abilities { get; set; } = [];
    public ICollection<Series> Series { get; set; } = [];
    public ICollection<CharacterRelationship> Relationships { get; set; } = [];
}
