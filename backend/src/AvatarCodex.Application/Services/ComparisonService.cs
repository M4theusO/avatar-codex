using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;

namespace AvatarCodex.Application.Services;

/// <summary>Comparação de dois personagens (RF-COMP-01/02): informações lado a lado e atributos estimados.</summary>
public class ComparisonService(IComparisonRepository repository, IValidator<CharacterComparisonQuery> validator)
{
    public async Task<CharacterComparisonDto> CompareAsync(
        CharacterComparisonQuery query, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var ids = query.ParseIds()!;

        var found = await repository.GetCharactersAsync(ids, cancellationToken);
        var characters = ids
            .Select(id => found.FirstOrDefault(c => c.Id == id) ?? throw NotFoundException.For("Personagem", id))
            .ToList();
        var attributes = await repository.GetAttributesAsync(cancellationToken);

        return new CharacterComparisonDto(
            [.. characters.Select(c => c.ToComparedDto())],
            [.. attributes.Select(attribute =>
            {
                var values = characters
                    .Select(c => c.AttributeValues.FirstOrDefault(v => v.ComparisonAttributeId == attribute.Id)?.Value)
                    .ToList();
                return new AttributeComparisonDto(
                    attribute.Id, attribute.Name, attribute.Description, values, Leader(characters, values));
            })]);
    }

    /// <summary>Quem tem o maior valor; ninguém se houver empate ou se faltar algum valor.</summary>
    public static int? Leader(IReadOnlyList<Character> characters, IReadOnlyList<int?> values)
    {
        if (values.Any(v => v is null))
            return null;

        var max = values.Max();
        var leaders = characters.Where((_, index) => values[index] == max).ToList();
        return leaders.Count == 1 ? leaders[0].Id : null;
    }
}
