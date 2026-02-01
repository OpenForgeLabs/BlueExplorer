using System.Collections.Concurrent;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Commons.Models;
using Commons.Results;
using FluentResults;
using Microsoft.Extensions.Options;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace ServiceBus.Infrastructure.Services;

public class ServiceBusConnectionProvider : IServiceBusConnectionProvider
{
    private readonly ConcurrentDictionary<string, string> _secretCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public ServiceBusConnectionProvider(IOptionsMonitor<ServiceBusOptions> options)
    {
        _options = options;
    }

    public Result<IReadOnlyList<ConnectionInfo>> GetConnections()
    {
        try
        {
            IReadOnlyList<ConnectionInfo> items = _options.CurrentValue.Connections
                .Select(config => new ConnectionInfo(config.Name, config.UseManagedIdentity))
                .ToList();
            return Result.Ok(items);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to load connections.", new[] { ex.Message }));
        }
    }

    public async Task<Result<ServiceBusConnection>> GetConnectionAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            ServiceBusConnectionConfig? config = _options.CurrentValue.Connections
                .FirstOrDefault(conn => conn.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (config == null)
                return Result.Fail(new HandledFail("Connection not found.", new[] { $"No connection configured with name '{name}'." }));

            string? connectionString = config.ConnectionString;
            if (config.KeyVault != null)
                connectionString = await GetSecretAsync(config, cancellationToken);

            if (string.IsNullOrWhiteSpace(connectionString))
                return Result.Fail(new HandledFail("Invalid connection.", new[] { $"Connection '{name}' does not have a valid connection string." }));

            return Result.Ok(new ServiceBusConnection(config.Name, connectionString, config.UseManagedIdentity));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to resolve connection.", new[] { ex.Message }));
        }
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
