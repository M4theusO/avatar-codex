using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace AvatarCodex.Application.Services;

/// <summary>Ações sobre a própria conta que exigem confirmar a senha.</summary>
public class AccountService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IValidator<DeleteAccountRequest> deleteValidator)
{
    public const string WrongPasswordMessage = "Senha incorreta.";
    public const string DemoAccountMessage = "A conta de demonstração é compartilhada e não pode ser excluída.";

    /// <summary>
    /// Exclui a conta e tudo o que pertence a ela (progresso, favoritos, descobertas, quizzes, sessões).
    /// Senha errada vira erro de validação (400), não 401: a sessão continua válida.
    /// </summary>
    public async Task DeleteAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken = default)
    {
        await deleteValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException(AuthService.InvalidSessionMessage);

        if (!passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new ValidationException([new ValidationFailure(nameof(request.Password), WrongPasswordMessage)]);

        if (string.Equals(user.Email, DemoAccount.Email, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException(DemoAccountMessage);

        await users.DeleteAsync(userId, cancellationToken);
    }
}
