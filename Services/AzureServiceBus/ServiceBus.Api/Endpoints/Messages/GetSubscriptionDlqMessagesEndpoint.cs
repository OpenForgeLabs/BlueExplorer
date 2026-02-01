
using Commons.Api;
using Commons.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Messages;

public static class GetSubscriptionDlqMessagesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/dlq", HandleAsync)
            .Produces<ApiResponse<PagedResult<MessageDto>>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Messages");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        [FromQuery] long? continuationToken,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100
        )
    {
        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.GetSubscriptionMessagesAsync(
                connection,
                topicName,
                subscriptionName,
                true,
                pageSize,
                continuationToken,
                cancellationToken));
    }
}
