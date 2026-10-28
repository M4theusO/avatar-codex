using AvatarCodex.Application.DTOs;
using FluentValidation;

namespace AvatarCodex.Application.Validators;

public class CharacterComparisonQueryValidator : AbstractValidator<CharacterComparisonQuery>
{
    public const int CharacterCount = 2;

    public CharacterComparisonQueryValidator()
    {
        RuleFor(q => q.Ids)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe os dois personagens a comparar (ex.: ids=1,2).")
            .Must(BeTwoPositiveIds)
            .WithMessage("Informe exatamente dois ids positivos separados por vírgula (ex.: ids=1,2).")
            .Must(BeDifferent).WithMessage("Escolha dois personagens diferentes.");
    }

    private static bool BeTwoPositiveIds(CharacterComparisonQuery query, string? _) =>
        query.ParseIds() is { Length: CharacterCount } ids && ids.All(id => id > 0);

    private static bool BeDifferent(CharacterComparisonQuery query, string? _) =>
        query.ParseIds()!.Distinct().Count() == CharacterCount;
}
