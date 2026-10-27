using AvatarCodex.Application.DTOs;
using AvatarCodex.Domain.Enums;
using FluentValidation;

namespace AvatarCodex.Application.Validators;

public class QuizQueryParametersValidator : AbstractValidator<QuizQueryParameters>
{
    public const int MaxCategoryLength = 50;

    public QuizQueryParametersValidator()
    {
        RuleFor(q => q.Category)
            .MaximumLength(MaxCategoryLength).WithMessage($"A categoria deve ter no máximo {MaxCategoryLength} caracteres.");

        RuleFor(q => q.Difficulty)
            .Must(d => d is null || (Enum.TryParse<QuizDifficulty>(d, ignoreCase: true, out _) && !int.TryParse(d, out _)))
            .WithMessage("Dificuldade inválida. Use: Easy, Medium ou Hard.");
    }
}
