using AvatarCodex.Application.DTOs;
using FluentValidation;

namespace AvatarCodex.Application.Validators;

public class TimelineQueryParametersValidator : AbstractValidator<TimelineQueryParameters>
{
    public TimelineQueryParametersValidator()
    {
        RuleFor(q => q.EraId)
            .GreaterThan(0).When(q => q.EraId is not null).WithMessage("A era deve ser um id positivo.");

        RuleFor(q => q.CharacterId)
            .GreaterThan(0).When(q => q.CharacterId is not null).WithMessage("O personagem deve ser um id positivo.");
    }
}
