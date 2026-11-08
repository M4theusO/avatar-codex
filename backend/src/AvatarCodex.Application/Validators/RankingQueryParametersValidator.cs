using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Services;
using FluentValidation;

namespace AvatarCodex.Application.Validators;

public class RankingQueryParametersValidator : AbstractValidator<RankingQueryParameters>
{
    public RankingQueryParametersValidator()
    {
        RuleFor(q => q.Period)
            .Must(p => RankingRules.TryParsePeriod(p, out _))
            .WithMessage($"Período inválido. Use: {RankingRules.AllTime} ou {RankingRules.Month}.");

        RuleFor(q => q.ElementId)
            .GreaterThan(0).When(q => q.ElementId is not null)
            .WithMessage("O elemento deve ser maior que zero.");

        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1).WithMessage("A página deve ser maior ou igual a 1.");

        RuleFor(q => q.PageSize)
            .InclusiveBetween(1, CharacterQueryParametersValidator.MaxPageSize)
            .WithMessage($"O tamanho da página deve estar entre 1 e {CharacterQueryParametersValidator.MaxPageSize}.");
    }
}
