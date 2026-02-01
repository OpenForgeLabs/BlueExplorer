using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Messages;

public static class SendQueueMessageEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/queues/{queueName}/messages", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("ServiceBus Messages");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string queueName,
        [FromBody] SendMessageRequest request,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] IQueueService queueService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return ApiResults.BadRequest("Content is required.");

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => queueService.SendMessageAsync(connection, queueName, request.Content, cancellationToken));
    }
}
