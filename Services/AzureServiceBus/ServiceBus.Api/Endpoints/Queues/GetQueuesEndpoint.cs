
using Commons.Api;
using Commons.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Queues;

public static class GetQueuesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/queues", HandleAsync)
            .Produces<ApiResponse<PagedResult<QueueInfo>>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Queues");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] IQueueService queueService,
        [FromQuery] string? continuationToken,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100)
    {
        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => queueService.GetQueuesAsync(connection, pageSize, continuationToken, cancellationToken));
    }
}
