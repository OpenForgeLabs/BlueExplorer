
using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace BlueExplorer.ServiceBus.Api.Endpoints;

internal static class ServiceBusEndpointHelpers
{
    public static IResult? RequireConfirmation(string? confirmation, string expected, string resourceLabel)
    {
        if (!string.IsNullOrWhiteSpace(confirmation) &&
            confirmation.Equals(expected, StringComparison.Ordinal))
            return null;

        return ApiResults.BadRequest(
            $"Confirmation required. Add ?confirmName={expected} to confirm deleting {resourceLabel}.");
    }

    public static Task<FluentResults.Result<ServiceBusConnection>> ResolveConnection(
        IServiceBusConnectionProvider connectionProvider,
        string connectionName,
        CancellationToken cancellationToken)
    {
        return connectionProvider.GetConnectionAsync(connectionName, cancellationToken);
    }

    public static async Task<IResult> WithConnection(
        IServiceBusConnectionProvider connectionProvider,
        string connectionName,
        CancellationToken cancellationToken,
        Func<ServiceBusConnection, Task<FluentResults.Result>> action)
    {
        FluentResults.Result<ServiceBusConnection> connectionResult =
            await ResolveConnection(connectionProvider, connectionName, cancellationToken);
        if (connectionResult.IsFailed)
            return ApiResults.FromResult(connectionResult);

        FluentResults.Result result = await action(connectionResult.Value);
        return ApiResults.FromResult(result);
    }

    public static async Task<IResult> WithConnection<T>(
        IServiceBusConnectionProvider connectionProvider,
        string connectionName,
        CancellationToken cancellationToken,
        Func<ServiceBusConnection, Task<FluentResults.Result<T>>> action)
    {
        FluentResults.Result<ServiceBusConnection> connectionResult =
            await ResolveConnection(connectionProvider, connectionName, cancellationToken);
        if (connectionResult.IsFailed)
            return ApiResults.FromResult(connectionResult);

        FluentResults.Result<T> result = await action(connectionResult.Value);
        return ApiResults.FromResult(result);
    }

    public static string ValidateConnectionRequest(ConnectionUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Name is required.";

        bool hasConn = !string.IsNullOrWhiteSpace(request.ConnectionString);
        bool hasKeyVault = request.KeyVault != null &&
                           !string.IsNullOrWhiteSpace(request.KeyVault.VaultUri) &&
                           !string.IsNullOrWhiteSpace(request.KeyVault.SecretName);
        if (!hasConn && !hasKeyVault)
            return "Either ConnectionString or KeyVault settings are required.";

        return string.Empty;
    }
}
