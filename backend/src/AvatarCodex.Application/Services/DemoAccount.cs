using AvatarCodex.Domain.Entities;

namespace AvatarCodex.Application.Services;

/// <summary>Conta de demonstração compartilhada pelos visitantes (credenciais no README).</summary>
public static class DemoAccount
{
    public const string Email = "demo@avatarcodex.dev";
    public const string Username = "demo";
    public const string Password = "Avatar123";

    /// <summary>A conta demo é compartilhada: não pode ser excluída nem ter nome ou foto alterados.</summary>
    public static bool Is(User user) => string.Equals(user.Email, Email, StringComparison.OrdinalIgnoreCase);
}
