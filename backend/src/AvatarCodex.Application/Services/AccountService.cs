using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace AvatarCodex.Application.Services;

/// <summary>Ações sobre a própria conta: editar o perfil e excluir a conta (esta com a senha).</summary>
public class AccountService(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IValidator<DeleteAccountRequest> deleteValidator,
    IValidator<UpdateProfileRequest> updateValidator,
    IValidator<UpdatePrivacyRequest> privacyValidator)
{
    public const string WrongPasswordMessage = "Senha incorreta.";
    public const string DemoAccountMessage = "A conta de demonstração é compartilhada e não pode ser excluída.";
    public const string DemoProfileMessage = "A conta de demonstração é compartilhada e o nome dela não pode ser alterado.";
    public const string UsernameTakenMessage = "Este nome de usuário já está em uso.";
    public const string DemoPrivacyMessage = "A conta de demonstração é compartilhada e o perfil dela continua visível.";

    /// <summary>
    /// Torna o perfil público (para quem está logado) ou privado. Repetir o estado atual não é erro;
    /// a conta demo fica sempre visível.
    /// </summary>
    public async Task<UserDto> UpdatePrivacyAsync(Guid userId, UpdatePrivacyRequest request, CancellationToken cancellationToken = default)
    {
        await privacyValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetForUpdateAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException(AuthService.InvalidSessionMessage);

        var isPrivate = request.IsProfilePrivate!.Value;
        if (user.IsProfilePrivate == isPrivate)
            return user.ToDto();

        if (DemoAccount.Is(user))
            throw new BusinessRuleException(DemoPrivacyMessage);

        user.IsProfilePrivate = isPrivate;
        await users.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    /// <summary>
    /// Troca o nome de usuário. Mudar só maiúsculas/minúsculas do próprio nome é permitido;
    /// nome de outra pessoa → 409 (a unique do banco cobre a corrida).
    /// </summary>
    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        request = request with { Username = request.Username?.Trim() ?? "" };
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await users.GetForUpdateAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException(AuthService.InvalidSessionMessage);

        if (user.Username == request.Username)
            return user.ToDto();

        if (DemoAccount.Is(user))
            throw new BusinessRuleException(DemoProfileMessage);

        // citext no banco: "Aang" e "aang" são o mesmo nome. Só consulta se o nome é de outra pessoa.
        if (!string.Equals(user.Username, request.Username, StringComparison.OrdinalIgnoreCase)
            && await users.UsernameExistsAsync(request.Username, cancellationToken))
            throw new ConflictException(UsernameTakenMessage);

        user.Username = request.Username;
        await users.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

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

        if (DemoAccount.Is(user))
            throw new BusinessRuleException(DemoAccountMessage);

        await users.DeleteAsync(userId, cancellationToken);
    }
}
