using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Services;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Connections;

public static class GetConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}", HandleAsync)
            .Produces<ApiResponse<ConnectionUpsertRequest>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .WithTags("ServiceBus Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IOptionsMonitor<ServiceBusOptions> options,
        [FromServices] IConnectionStore connectionStore,
        CancellationToken cancellationToken)
    {
        var storeResult = await connectionStore.GetAsync(connectionName, cancellationToken);
        if (storeResult.IsSuccess)
        {
            ServiceBusConnectionConfig config = storeResult.Value;
            return ApiResults.Ok(new ConnectionUpsertRequest
            {
                Name = config.Name,
                UseManagedIdentity = config.UseManagedIdentity,
                ConnectionString = config.ConnectionString,
                KeyVault = config.KeyVault
            });
        }

        if (storeResult.IsFailed &&
            !storeResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.FromResult(storeResult);
        }

        ServiceBusConnectionConfig? fromSettings = options.CurrentValue.Connections
            .FirstOrDefault(conn => conn.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
        if (fromSettings == null)
            return ApiResults.NotFound();

        return ApiResults.Ok(new ConnectionUpsertRequest
        {
            Name = fromSettings.Name,
            UseManagedIdentity = fromSettings.UseManagedIdentity,
            ConnectionString = null,
            KeyVault = fromSettings.KeyVault
        });
    }
}
