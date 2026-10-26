using System.Text;

namespace AvatarCodex.Infrastructure.Security;

/// <summary>Seção "Jwt" da configuração. O Secret vem de user-secrets ou de variável de ambiente (Jwt__Secret).</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 exige chave de pelo menos 256 bits.</summary>
    public const int MinimumSecretBytes = 32;

    public string Secret { get; set; } = "";
    public string Issuer { get; set; } = "AvatarCodex";
    public string Audience { get; set; } = "AvatarCodex";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;

    public byte[] GetSigningKey()
    {
        var key = Encoding.UTF8.GetBytes(Secret);
        if (key.Length < MinimumSecretBytes)
            throw new InvalidOperationException(
                $"Jwt:Secret não configurado ou com menos de {MinimumSecretBytes} bytes. Veja a seção 'Variáveis de ambiente' do README.");
        return key;
    }
}
