using ServiceBus.Application.Configuration;

namespace BlueExplorer.ServiceBus.Api.Contracts;

public class ConnectionUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public bool UseManagedIdentity { get; set; }
    public string? ConnectionString { get; set; }
    public KeyVaultSecretConfig? KeyVault { get; set; }
}
