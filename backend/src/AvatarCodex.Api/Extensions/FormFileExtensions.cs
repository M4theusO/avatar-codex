namespace AvatarCodex.Api.Extensions;

public static class FormFileExtensions
{
    /// <summary>Conteúdo do arquivo enviado; sem arquivo, um array vazio (o serviço responde "Escolha uma imagem").</summary>
    public static async Task<byte[]> ReadBytesAsync(this IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
            return [];
        using var stream = new MemoryStream((int)file.Length);
        await file.CopyToAsync(stream, cancellationToken);
        return stream.ToArray();
    }
}
