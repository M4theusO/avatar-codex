namespace AvatarCodex.Domain.Entities;

/// <summary>Pergunta do teste de elemento (RF-ELTEST-01). Order é a chave natural usada pelo seed.</summary>
public class ElementTestQuestion
{
    public int Id { get; set; }
    public required string Text { get; set; }
    public int Order { get; set; }
    public List<ElementTestOption> Options { get; set; } = [];
}

/// <summary>Cada opção vale um ponto para o seu elemento; cada pergunta tem uma opção por elemento.</summary>
public class ElementTestOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public required string Text { get; set; }
    public int Order { get; set; }

    public int ElementId { get; set; }
    public Element Element { get; set; } = null!;
}
