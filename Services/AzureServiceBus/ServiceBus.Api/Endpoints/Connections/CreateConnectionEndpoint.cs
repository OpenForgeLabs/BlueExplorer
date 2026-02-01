using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Services;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Connections;

public static class CreateConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost(string.Empty, HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict)
            .WithTags("ServiceBus Connections");
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] ConnectionUpsertRequest request,
        [FromServices] IOptionsMonitor<ServiceBusOptions> options,
        [FromServices] IConnectionStore connectionStore,
        CancellationToken cancellationToken)
    {
        string validation = ServiceBusEndpointHelpers.ValidateConnectionRequest(request);
        if (!string.IsNullOrWhiteSpace(validation))
            return ApiResults.BadRequest(validation);

        bool existsInSettings = options.CurrentValue.Connections
            .Any(conn => conn.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
        if (existsInSettings)
            return ApiResults.Conflict($"Connection '{request.Name}' already exists in appsettings.");

        var config = new ServiceBusConnectionConfig
        {
            Name = request.Name,
            UseManagedIdentity = request.UseManagedIdentity,
            ConnectionString = request.ConnectionString,
            KeyVault = request.KeyVault
        };

        var addResult = await connectionStore.AddAsync(config, cancellationToken);
        if (addResult.IsFailed &&
            addResult.Errors.Any(error => error.Message == "Connection already exists."))
        {
            return ApiResults.Conflict($"Connection '{request.Name}' already exists in connections.json.");
        }

        return ApiResults.FromResult(addResult);
    }
}
