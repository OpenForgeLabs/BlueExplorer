namespace PurpleExplorer.Web.Models;

public class ConnectionUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public bool UseManagedIdentity { get; set; }
    public string? ConnectionString { get; set; }
    public KeyVaultSecretConfig? KeyVault { get; set; }
}

public class KeyVaultSecretConfig
{
    public string VaultUri { get; set; } = string.Empty;
    public string SecretName { get; set; } = string.Empty;
    public string? SecretVersion { get; set; }
}
