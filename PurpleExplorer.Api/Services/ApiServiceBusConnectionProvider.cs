using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Linq;
using CoreConnectionInfo = PurpleExplorer.Core.Models.ConnectionInfo;
using Microsoft.Extensions.Options;
using PurpleExplorer.Core.Configuration;
using PurpleExplorer.Core.Models;
using PurpleExplorer.Core.Services;

namespace PurpleExplorer.Api.Services;

public class ApiServiceBusConnectionProvider : IServiceBusConnectionProvider
{
    private readonly IConnectionStore _store;
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public ApiServiceBusConnectionProvider(
        IConnectionStore store,
        IOptionsMonitor<ServiceBusOptions> options)
    {
        _store = store;
        _options = options;
    }

    public IReadOnlyList<CoreConnectionInfo> GetConnections()
    {
        IEnumerable<ServiceBusConnectionConfig> allConfigs =
            _options.CurrentValue.Connections.Concat(_store.GetAllAsync().GetAwaiter().GetResult());
        return allConfigs
            .Select(config => new CoreConnectionInfo(config.Name, config.UseManagedIdentity))
            .ToList();
    }

    public async Task<ServiceBusConnection> GetConnectionAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ServiceBusConnectionConfig? config = await GetConfigAsync(name, cancellationToken);
        if (config == null)
            throw new KeyNotFoundException($"No Service Bus connection configured with name '{name}'.");

        string? connectionString = config.ConnectionString;
        if (config.KeyVault != null)
            connectionString = await GetSecretAsync(config, cancellationToken);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection '{name}' does not have a valid connection string.");

        return new ServiceBusConnection(config.Name, connectionString, config.UseManagedIdentity);
    }

    public async Task<ServiceBusConnectionConfig?> GetConfigAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ServiceBusConnectionConfig? config = _options.CurrentValue.Connections
            .FirstOrDefault(conn => conn.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (config != null)
            return config;

        return await _store.GetAsync(name, cancellationToken);
    }

    private static async Task<string> GetSecretAsync(
        ServiceBusConnectionConfig config,
        CancellationToken cancellationToken)
    {
        KeyVaultSecretConfig keyVault = config.KeyVault!;
        if (string.IsNullOrWhiteSpace(keyVault.VaultUri))
            throw new InvalidOperationException($"Connection '{config.Name}' is missing KeyVault.VaultUri.");
        if (string.IsNullOrWhiteSpace(keyVault.SecretName))
            throw new InvalidOperationException($"Connection '{config.Name}' is missing KeyVault.SecretName.");

        var client = new SecretClient(new Uri(keyVault.VaultUri), new DefaultAzureCredential());
        KeyVaultSecret secret = string.IsNullOrWhiteSpace(keyVault.SecretVersion)
            ? await client.GetSecretAsync(keyVault.SecretName, cancellationToken: cancellationToken)
            : await client.GetSecretAsync(keyVault.SecretName, keyVault.SecretVersion, cancellationToken);

        return secret.Value;
    }
}
