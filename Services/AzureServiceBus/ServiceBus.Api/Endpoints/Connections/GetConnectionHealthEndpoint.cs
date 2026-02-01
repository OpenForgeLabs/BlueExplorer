using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Connections;

public static class GetConnectionHealthEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/health", HandleAsync)
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        CancellationToken cancellationToken)
    {
        var connectionResult = await ServiceBusEndpointHelpers.ResolveConnection(connectionProvider, connectionName, cancellationToken);
        if (connectionResult.IsFailed)
            return ApiResults.Ok(false);

        var infoResult = await topicService.GetNamespaceInfoAsync(connectionResult.Value, cancellationToken);
        return ApiResults.Ok(infoResult.IsSuccess);
    }
}
