namespace AvatarCodex.Application.Services;

/// <summary>Regras da foto de perfil: tamanho máximo, tipos aceitos (pelo conteúdo, não pela extensão) e URL pública.</summary>
public static class AvatarRules
{
    /// <summary>O frontend reduz a foto para 256×256 antes de enviar; 512 KB sobra com folga.</summary>
    public const int MaxBytes = 512 * 1024;

    /// <summary>Limite do corpo da requisição (arquivo + envelope do multipart).</summary>
    public const int MaxRequestBytes = MaxBytes + 16 * 1024;

    /// <summary>Tipo da imagem pelos primeiros bytes (assinatura do formato), ou null se não for JPEG, PNG ou WebP.</summary>
    public static string? DetectContentType(ReadOnlySpan<byte> data)
    {
        if (data.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
            return "image/jpeg";
        if (data.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
            return "image/png";
        if (data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data[8..12].SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }

    public static string UrlFor(Guid publicId) => $"/api/avatars/{publicId:N}";
}
