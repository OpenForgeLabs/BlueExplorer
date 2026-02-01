using Commons.Api;
using Commons.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Subscriptions;

public static class GetSubscriptionsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/topics/{topicName}/subscriptions", HandleAsync)
            .Produces<ApiResponse<PagedResult<SubscriptionInfo>>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Subscriptions");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string topicName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        [FromQuery] string? continuationToken,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100)
    {
        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.GetSubscriptionsAsync(connection, topicName, pageSize, continuationToken, cancellationToken));
    }
}
