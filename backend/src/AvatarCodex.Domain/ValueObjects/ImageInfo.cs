namespace AvatarCodex.Domain.ValueObjects;

/// <summary>Imagem com metadados de origem, para poder ser substituída e creditada.</summary>
public class ImageInfo
{
    public required string Url { get; set; }
    public string? SourceUrl { get; set; }
    public string? Credit { get; set; }
}
