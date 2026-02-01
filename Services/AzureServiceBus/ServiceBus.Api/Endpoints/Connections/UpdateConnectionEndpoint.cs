using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Services;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Connections;

public static class UpdateConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("{connectionName}", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .WithTags("ServiceBus Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromBody] ConnectionUpsertRequest request,
        [FromServices] IConnectionStore connectionStore,
        CancellationToken cancellationToken)
    {
        if (!connectionName.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            return ApiResults.BadRequest("Connection name mismatch.");

        string validation = ServiceBusEndpointHelpers.ValidateConnectionRequest(request);
        if (!string.IsNullOrWhiteSpace(validation))
            return ApiResults.BadRequest(validation);

        var config = new ServiceBusConnectionConfig
        {
            Name = request.Name,
            UseManagedIdentity = request.UseManagedIdentity,
            ConnectionString = request.ConnectionString,
            KeyVault = request.KeyVault
        };

        var updateResult = await connectionStore.UpdateAsync(config, cancellationToken);
        if (updateResult.IsFailed &&
            updateResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.NotFound();
        }

        return ApiResults.FromResult(updateResult);
    }
}
