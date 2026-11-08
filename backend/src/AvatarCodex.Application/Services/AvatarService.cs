using AvatarCodex.Application.DTOs;
using AvatarCodex.Application.Interfaces;
using AvatarCodex.Application.Mappings;
using AvatarCodex.Domain.Entities;
using AvatarCodex.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;

namespace AvatarCodex.Application.Services;

/// <summary>Foto de perfil: enviar (substitui a anterior), remover e servir.</summary>
public class AvatarService(IUserRepository users, IUserAvatarRepository avatars)
{
    public const string FileField = "File";
    public const string EmptyFileMessage = "Escolha uma imagem.";
    public static readonly string TooLargeMessage = $"A imagem deve ter no máximo {AvatarRules.MaxBytes / 1024} KB.";
    public const string InvalidTypeMessage = "Envie uma imagem JPEG, PNG ou WebP.";
    public const string DemoAccountMessage = "A conta de demonstração é compartilhada e a foto dela não pode ser alterada.";
    public const string NotFoundMessage = "Foto não encontrada.";

    /// <summary>Grava a foto do usuário. Se já existir uma, a mesma linha é sobrescrita (com um novo id público).</summary>
    public async Task<UserDto> UploadAsync(Guid userId, byte[] data, CancellationToken cancellationToken = default)
    {
        var contentType = Validate(data);
        var user = await GetUserAsync(userId, cancellationToken);
        if (DemoAccount.Is(user))
            throw new BusinessRuleException(DemoAccountMessage);

        var avatar = await avatars.GetByUserIdAsync(userId, cancellationToken);
        if (avatar is null)
        {
            avatar = new UserAvatar { UserId = userId, ContentType = contentType, Data = data };
            avatars.Add(avatar);
        }
        else
        {
            avatar.ContentType = contentType;
            avatar.Data = data;
        }
        avatar.PublicId = Guid.NewGuid();
        user.AvatarUrl = AvatarRules.UrlFor(avatar.PublicId);

        await users.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    /// <summary>Remove a foto (volta para as iniciais). Sem foto, não faz nada.</summary>
    public async Task<UserDto> RemoveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken);
        var avatar = await avatars.GetByUserIdAsync(userId, cancellationToken);
        if (avatar is not null)
        {
            if (DemoAccount.Is(user))
                throw new BusinessRuleException(DemoAccountMessage);
            avatars.Remove(avatar);
        }
        user.AvatarUrl = null;
        await users.SaveChangesAsync(cancellationToken);
        return user.ToDto();
    }

    public async Task<UserAvatar> GetAsync(Guid publicId, CancellationToken cancellationToken = default) =>
        await avatars.GetByPublicIdAsync(publicId, cancellationToken)
            ?? throw new NotFoundException(NotFoundMessage);

    private static string Validate(byte[] data)
    {
        if (data.Length == 0)
            throw Invalid(EmptyFileMessage);
        if (data.Length > AvatarRules.MaxBytes)
            throw Invalid(TooLargeMessage);
        return AvatarRules.DetectContentType(data) ?? throw Invalid(InvalidTypeMessage);
    }

    private static ValidationException Invalid(string message) =>
        new([new ValidationFailure(FileField, message)]);

    private async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await users.GetForUpdateAsync(userId, cancellationToken)
            ?? throw new UnauthorizedException(AuthService.InvalidSessionMessage);
}
