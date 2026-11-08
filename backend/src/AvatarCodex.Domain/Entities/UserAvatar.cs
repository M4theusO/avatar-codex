using AvatarCodex.Domain.Interfaces;

namespace AvatarCodex.Domain.Entities;

/// <summary>
/// Foto de perfil. A chave é o próprio UserId: cada usuário tem no máximo uma, e enviar outra
/// substitui a anterior na mesma linha. Fica no banco porque o disco do servidor não é persistente.
/// </summary>
public class UserAvatar : IAuditableEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Id público usado na URL; muda a cada envio, então a URL antiga deixa de valer e o cache não serve a foto velha.</summary>
    public Guid PublicId { get; set; }

    public required string ContentType { get; set; }
    public required byte[] Data { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
