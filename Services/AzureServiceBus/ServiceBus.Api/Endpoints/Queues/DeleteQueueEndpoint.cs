
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Queues;

public static class DeleteQueueEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("{connectionName}/queues/{queueName}", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Queues");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string queueName,
        [FromQuery] string? confirmName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] IQueueService queueService,
        CancellationToken cancellationToken)
    {
        IResult? confirmation = ServiceBusEndpointHelpers.RequireConfirmation(confirmName, queueName, "queue");
        if (confirmation != null)
            return confirmation;

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => queueService.DeleteQueueAsync(connection, queueName, cancellationToken));
    }
}
