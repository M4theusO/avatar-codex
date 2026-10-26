namespace AvatarCodex.Domain.Entities;

// Registros do usuário sobre o conteúdo. As PKs compostas (UserId + item) impedem duplicidade.

public class UserCharacterDiscovery
{
    public Guid UserId { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public DateTimeOffset DiscoveredAt { get; set; }
}

public class UserNationDiscovery
{
    public Guid UserId { get; set; }
    public int NationId { get; set; }
    public Nation Nation { get; set; } = null!;
    public DateTimeOffset DiscoveredAt { get; set; }
}

public class UserFavoriteCharacter
{
    public Guid UserId { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
