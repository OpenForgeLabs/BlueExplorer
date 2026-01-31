using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using PurpleExplorer.Core.Configuration;
using PurpleExplorer.Core.Models;

namespace PurpleExplorer.Core.Services;

public class ServiceBusConnectionProvider : IServiceBusConnectionProvider
{
    private readonly ConcurrentDictionary<string, string> _secretCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public ServiceBusConnectionProvider(IOptionsMonitor<ServiceBusOptions> options)
    {
        _options = options;
    }

    public IReadOnlyList<ConnectionInfo> GetConnections()
    {
        return _options.CurrentValue.Connections
            .Select(config => new ConnectionInfo(config.Name, config.UseManagedIdentity))
            .ToList();
    }

    public async Task<ServiceBusConnection> GetConnectionAsync(string name, CancellationToken cancellationToken = default)
    {
        ServiceBusConnectionConfig? config = _options.CurrentValue.Connections
            .FirstOrDefault(conn => conn.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (config == null)
            throw new KeyNotFoundException($"No Service Bus connection configured with name '{name}'.");

        string? connectionString = config.ConnectionString;
        if (config.KeyVault != null)
            connectionString = await GetSecretAsync(config, cancellationToken);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"Connection '{name}' does not have a valid connection string.");

        return new ServiceBusConnection(config.Name, connectionString, config.UseManagedIdentity);
    }

    private async Task<string> GetSecretAsync(ServiceBusConnectionConfig config, CancellationToken cancellationToken)
    {
        string cacheKey = config.Name;
        if (_secretCache.TryGetValue(cacheKey, out string? cached))
            return cached;

        KeyVaultSecretConfig keyVault = config.KeyVault!;
        if (string.IsNullOrWhiteSpace(keyVault.VaultUri))
            throw new InvalidOperationException($"Connection '{config.Name}' is missing KeyVault.VaultUri.");
        if (string.IsNullOrWhiteSpace(keyVault.SecretName))
            throw new InvalidOperationException($"Connection '{config.Name}' is missing KeyVault.SecretName.");

        var client = new SecretClient(new Uri(keyVault.VaultUri), new DefaultAzureCredential());
        KeyVaultSecret secret = string.IsNullOrWhiteSpace(keyVault.SecretVersion)
            ? await client.GetSecretAsync(keyVault.SecretName, cancellationToken: cancellationToken)
            : await client.GetSecretAsync(keyVault.SecretName, keyVault.SecretVersion, cancellationToken);

        _secretCache[cacheKey] = secret.Value;
        return secret.Value;
    }
}
