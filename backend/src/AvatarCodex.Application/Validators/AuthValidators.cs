using AvatarCodex.Application.DTOs;
using FluentValidation;

namespace AvatarCodex.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 30;
    public const int EmailMaxLength = 254;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;

    public RegisterRequestValidator()
    {
        RuleFor(r => r.Username)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe um nome de usuário.")
            .Length(UsernameMinLength, UsernameMaxLength)
                .WithMessage($"O nome de usuário deve ter entre {UsernameMinLength} e {UsernameMaxLength} caracteres.")
            .Matches("^[A-Za-z0-9_.-]+$")
                .WithMessage("Use apenas letras sem acento, números, ponto, hífen ou sublinhado.");

        RuleFor(r => r.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe seu email.")
            .MaximumLength(EmailMaxLength).WithMessage("Email muito longo.")
            .EmailAddress().WithMessage("Email inválido.");

        // RN-02: mínimo de 8 caracteres, com letra e número.
        RuleFor(r => r.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Informe uma senha.")
            .MinimumLength(PasswordMinLength).WithMessage($"A senha deve ter pelo menos {PasswordMinLength} caracteres.")
            .MaximumLength(PasswordMaxLength).WithMessage($"A senha deve ter no máximo {PasswordMaxLength} caracteres.")
            .Must(p => p.Any(char.IsLetter) && p.Any(char.IsDigit))
                .WithMessage("A senha deve ter pelo menos uma letra e um número.");

        RuleFor(r => r.ConfirmPassword)
            .Equal(r => r.Password).WithMessage("As senhas não conferem.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().WithMessage("Informe seu email.");
        RuleFor(r => r.Password).NotEmpty().WithMessage("Informe sua senha.");
    }
}
