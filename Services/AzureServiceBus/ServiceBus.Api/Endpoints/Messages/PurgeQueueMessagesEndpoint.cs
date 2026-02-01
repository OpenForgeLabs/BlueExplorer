using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Messages;

public static class PurgeQueueMessagesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/queues/{queueName}/purge", HandleAsync)
            .Produces<ApiResponse<long>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Messages");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string queueName,
        [FromBody] PurgeRequest request,
        [FromQuery] string? confirmName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] IQueueService queueService,
        CancellationToken cancellationToken)
    {
        IResult? confirmation = ServiceBusEndpointHelpers.RequireConfirmation(confirmName, queueName, "queue purge");
        if (confirmation != null)
            return confirmation;

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => queueService.PurgeMessagesAsync(connection, queueName, request.IsDlq, cancellationToken));
    }
}
