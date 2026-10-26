namespace AvatarCodex.Domain.Entities;

public class Series
{
    public int Id { get; set; }

    /// <summary>Chave estável, ex. "ATLA", "LOK".</summary>
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<Character> Characters { get; set; } = [];
}
