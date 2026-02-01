
using Commons.Api;
using Commons.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Messages;

public static class GetQueueDlqMessagesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/queues/{queueName}/dlq", HandleAsync)
            .Produces<ApiResponse<PagedResult<MessageDto>>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Messages");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string queueName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] IQueueService queueService,
        [FromQuery] long? continuationToken,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100
        )
    {
        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => queueService.GetMessagesAsync(
                connection,
                queueName,
                true,
                pageSize,
                continuationToken,
                cancellationToken));
    }
}
