
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Connections;

public static class GetNamespaceInfoEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/namespace", HandleAsync)
            .Produces<ApiResponse<NamespaceInfo>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        CancellationToken cancellationToken)
    {
        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.GetNamespaceInfoAsync(connection, cancellationToken));
    }
}
