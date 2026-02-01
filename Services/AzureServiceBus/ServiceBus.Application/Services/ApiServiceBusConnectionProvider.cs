using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Commons.Models;
using Commons.Results;
using FluentResults;
using Microsoft.Extensions.Options;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace ServiceBus.Application.Services;

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

    public Result<IReadOnlyList<ConnectionInfo>> GetConnections()
    {
        try
        {
            Result<IReadOnlyList<ServiceBusConnectionConfig>> storedResult =
                _store.GetAllAsync().GetAwaiter().GetResult();
            
            if (storedResult.IsFailed)
            {
                IEnumerable<string> reasons = storedResult.Reasons.Select(reason => reason.Message);
                return Result.Fail(new HandledFail("Failed to load connections.", reasons));
            }

            IEnumerable<ServiceBusConnectionConfig> allConfigs =
                _options.CurrentValue.Connections.Concat(storedResult.Value);
            IReadOnlyList<ConnectionInfo> items = allConfigs
                .Select(config => new ConnectionInfo(config.Name, config.UseManagedIdentity))
                .ToList();
            return Result.Ok(items);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to load connections.", new[] { ex.Message }));
        }
    }

    public async Task<Result<ServiceBusConnection>> GetConnectionAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ServiceBusConnectionConfig? config = await GetConfigAsync(name, cancellationToken);
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

    public async Task<ServiceBusConnectionConfig?> GetConfigAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ServiceBusConnectionConfig? config = _options.CurrentValue.Connections
            .FirstOrDefault(conn => conn.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (config != null)
            return config;

        Result<ServiceBusConnectionConfig> storeResult = await _store.GetAsync(name, cancellationToken);
        if (storeResult.IsSuccess)
            return storeResult.Value;
        if (storeResult.Errors.Any(error => error.Message == "Connection not found."))
            return null;

        throw new InvalidOperationException(storeResult.Errors.First().Message);
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
