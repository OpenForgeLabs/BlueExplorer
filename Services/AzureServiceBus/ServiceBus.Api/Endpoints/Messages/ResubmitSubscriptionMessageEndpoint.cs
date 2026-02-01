using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Messages;

public static class ResubmitSubscriptionMessageEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/messages/resubmit", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("ServiceBus Messages");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromBody] MessageActionRequest request,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        CancellationToken cancellationToken)
    {
        if (request.SequenceNumber <= 0)
            return ApiResults.BadRequest("SequenceNumber is required.");

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.ResubmitDlqMessageAsync(
                connection,
                topicName,
                subscriptionName,
                request.SequenceNumber,
                cancellationToken));
    }
}
