using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Messages;

public static class DeadletterSubscriptionMessageEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/messages/deadletter", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("ServiceBus Messages");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromBody] MessageActionRequest request,
        [FromQuery] string? confirmName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MessageId))
            return ApiResults.BadRequest("MessageId is required.");

        IResult? confirmation = ServiceBusEndpointHelpers.RequireConfirmation(confirmName, request.MessageId, "message");
        if (confirmation != null)
            return confirmation;

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.DeadletterMessageAsync(connection, topicName, subscriptionName, request.MessageId, cancellationToken));
    }
}
