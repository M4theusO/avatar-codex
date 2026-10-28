namespace AvatarCodex.Domain.Entities;

/// <summary>
/// Atributo usado na comparação de personagens (ex.: "Estratégia"). Os valores são estimativas do projeto,
/// nunca informação canônica (RN-11). Order define a sequência de exibição.
/// </summary>
public class ComparisonAttribute
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Order { get; set; }
}

/// <summary>Valor de 0 a 100 de um personagem em um atributo de comparação.</summary>
public class CharacterAttributeValue
{
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public int ComparisonAttributeId { get; set; }
    public ComparisonAttribute ComparisonAttribute { get; set; } = null!;

    public int Value { get; set; }
}
