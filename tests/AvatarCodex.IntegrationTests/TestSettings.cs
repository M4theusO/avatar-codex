namespace AvatarCodex.IntegrationTests;

public static class TestSettings
{
    /// <summary>Chave usada só nos testes; em dev/produção vem de user-secrets ou variável de ambiente.</summary>
    public const string JwtSecret = "integration-tests-secret-key-with-at-least-32-bytes";
}
