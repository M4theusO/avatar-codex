using AvatarCodex.Application.DTOs;
using FluentValidation;

namespace AvatarCodex.Application.Validators;

public class PageQueryParametersValidator : AbstractValidator<PageQueryParameters>
{
    public PageQueryParametersValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1).WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, CharacterQueryParametersValidator.MaxPageSize)
            .WithMessage($"O tamanho da página deve estar entre 1 e {CharacterQueryParametersValidator.MaxPageSize}.");
    }
}
