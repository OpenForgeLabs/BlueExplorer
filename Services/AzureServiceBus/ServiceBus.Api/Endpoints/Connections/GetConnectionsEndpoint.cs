using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Services;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Connections;

public static class GetConnectionsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet(string.Empty, HandleAsync)
            .Produces<ApiResponse<IReadOnlyList<ConnectionSummary>>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Connections");
    }

    private static async Task<IResult> HandleAsync(
        [FromServices] IOptionsMonitor<ServiceBusOptions> options,
        [FromServices] IConnectionStore connectionStore,
        CancellationToken cancellationToken)
    {
        var storedResult = await connectionStore.GetAllAsync(cancellationToken);
        if (storedResult.IsFailed)
            return ApiResults.FromResult(storedResult);

        List<ConnectionSummary> items =
            options.CurrentValue.Connections.Select(config => new ConnectionSummary
                {
                    Name = config.Name,
                    UseManagedIdentity = config.UseManagedIdentity,
                    IsEditable = false,
                    Source = "appsettings"
                })
                .ToList();

        IReadOnlyList<ServiceBusConnectionConfig> stored = storedResult.Value;
        items.AddRange(stored.Select(config => new ConnectionSummary
        {
            Name = config.Name,
            UseManagedIdentity = config.UseManagedIdentity,
            IsEditable = true,
            Source = "connections.json"
        }));

        return ApiResults.Ok(items);
    }
}
