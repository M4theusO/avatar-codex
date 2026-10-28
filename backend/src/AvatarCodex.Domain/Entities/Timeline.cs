using AvatarCodex.Domain.ValueObjects;

namespace AvatarCodex.Domain.Entities;

/// <summary>Período da história (ex.: "A Guerra dos Cem Anos"). Order define a sequência cronológica.</summary>
public class Era
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Order { get; set; }

    public ICollection<TimelineEvent> Events { get; set; } = [];
}

/// <summary>Acontecimento da linha do tempo. Order é global (cronológico entre todas as eras).</summary>
public class TimelineEvent
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }

    /// <summary>Texto livre da data, ex.: "100 AG".</summary>
    public required string DateLabel { get; set; }

    public ImageInfo? Image { get; set; }
    public int Order { get; set; }

    public int EraId { get; set; }
    public Era Era { get; set; } = null!;

    public ICollection<Character> Characters { get; set; } = [];
}
