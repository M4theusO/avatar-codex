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
        RuleFor(r => r.Username).Cascade(CascadeMode.Stop).ValidUsername();

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

/// <summary>Regra do nome de usuário, a mesma no cadastro e na edição do perfil.</summary>
public static class UsernameRules
{
    public static IRuleBuilderOptions<T, string> ValidUsername<T>(this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty().WithMessage("Informe um nome de usuário.")
            .Length(RegisterRequestValidator.UsernameMinLength, RegisterRequestValidator.UsernameMaxLength)
                .WithMessage($"O nome de usuário deve ter entre {RegisterRequestValidator.UsernameMinLength} e {RegisterRequestValidator.UsernameMaxLength} caracteres.")
            .Matches("^[A-Za-z0-9_.-]+$")
                .WithMessage("Use apenas letras sem acento, números, ponto, hífen ou sublinhado.");
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(r => r.Username).Cascade(CascadeMode.Stop).ValidUsername();
    }
}

public class UpdatePrivacyRequestValidator : AbstractValidator<UpdatePrivacyRequest>
{
    public UpdatePrivacyRequestValidator()
    {
        RuleFor(r => r.IsProfilePrivate).NotNull().WithMessage("Informe se o perfil é privado.");
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

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(r => r.Password).NotEmpty().WithMessage("Informe sua senha para confirmar.");
    }
}
