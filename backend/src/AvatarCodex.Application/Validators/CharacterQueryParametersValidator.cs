using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Enums;
using FluentValidation;

namespace AvatarCodex.Application.Validators;

public class CharacterQueryParametersValidator : AbstractValidator<CharacterQueryParameters>
{
    public const int MaxPageSize = 50;
    public const int MaxSearchLength = 100;

    public CharacterQueryParametersValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1).WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, MaxPageSize).WithMessage($"O tamanho da página deve estar entre 1 e {MaxPageSize}.");

        RuleFor(q => q.Search)
            .MaximumLength(MaxSearchLength).WithMessage($"A busca deve ter no máximo {MaxSearchLength} caracteres.");

        RuleFor(q => q.NationId).GreaterThan(0).WithMessage("Nação inválida.");
        RuleFor(q => q.ElementId).GreaterThan(0).WithMessage("Elemento inválido.");
        RuleFor(q => q.SeriesId).GreaterThan(0).WithMessage("Série inválida.");

        RuleFor(q => q.Type)
            .Must(BeValidEnum<CharacterType>).WithMessage("Tipo inválido. Use: avatar, bender ou nonBender.");

        RuleFor(q => q.Status)
            .Must(BeValidEnum<CharacterStatus>).WithMessage("Status inválido. Use: Alive, Deceased ou Unknown.");

        RuleFor(q => q.Sort)
            .Must(BeValidEnum<CharacterSortField>).WithMessage("Ordenação inválida. Use: name, nation ou element.");

        RuleFor(q => q.Order)
            .Must(order => order is null || order.Equals("asc", StringComparison.OrdinalIgnoreCase)
                                         || order.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Direção inválida. Use: asc ou desc.");
    }

    private static bool BeValidEnum<TEnum>(string? value) where TEnum : struct, Enum =>
        value is null || (Enum.TryParse<TEnum>(value, ignoreCase: true, out _) && !int.TryParse(value, out _));
}
