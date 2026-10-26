namespace AvatarCodex.Domain.Interfaces;

/// <summary>Entidades com CreatedAt/UpdatedAt preenchidos automaticamente ao salvar.</summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset UpdatedAt { get; set; }
}
