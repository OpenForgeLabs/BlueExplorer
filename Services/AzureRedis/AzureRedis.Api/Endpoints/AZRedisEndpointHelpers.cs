using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using AzureRedis.Api.Contracts;
using Commons.Api;

namespace AzureRedis.Api.Endpoints;

internal static class AZRedisEndpointHelpers
{
    public static IResult? RequireConfirmation(string? confirmation, string expected, string resourceLabel)
    {
        if (!string.IsNullOrWhiteSpace(confirmation) &&
            confirmation.Equals(expected, StringComparison.Ordinal))
            return null;

        return ApiResults.BadRequest(
            $"Confirmation required. Add ?confirmName={expected} to confirm deleting {resourceLabel}.");
    }

    public static string ValidateConnectionRequest(RedisConnectionUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Name is required.";

        bool hasConnectionString = !string.IsNullOrWhiteSpace(request.ConnectionString);
        bool hasHost = !string.IsNullOrWhiteSpace(request.Host) && request.Port > 0;
        if (!hasConnectionString && !hasHost)
            return "Either ConnectionString or Host/Port is required.";

        return string.Empty;
    }

    public static Task<FluentResults.Result<RedisConnection>> ResolveConnection(
        IRedisConnectionProvider provider,
        string connectionName,
        CancellationToken cancellationToken)
    {
        return provider.GetConnectionAsync(connectionName, cancellationToken);
    }

    public static async Task<IResult> WithConnection(
        IRedisConnectionProvider provider,
        string connectionName,
        CancellationToken cancellationToken,
        Func<RedisConnection, Task<FluentResults.Result>> action)
    {
        FluentResults.Result<RedisConnection> connectionResult =
            await ResolveConnection(provider, connectionName, cancellationToken);
        if (connectionResult.IsFailed)
            return ApiResults.FromResult(connectionResult);

        FluentResults.Result result = await action(connectionResult.Value);
        return ApiResults.FromResult(result);
    }

    public static async Task<IResult> WithConnection<T>(
        IRedisConnectionProvider provider,
        string connectionName,
        CancellationToken cancellationToken,
        Func<RedisConnection, Task<FluentResults.Result<T>>> action)
    {
        FluentResults.Result<RedisConnection> connectionResult =
            await ResolveConnection(provider, connectionName, cancellationToken);
        if (connectionResult.IsFailed)
            return ApiResults.FromResult(connectionResult);

        FluentResults.Result<T> result = await action(connectionResult.Value);
        return ApiResults.FromResult(result);
    }
}
