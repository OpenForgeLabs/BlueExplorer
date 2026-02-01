using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Subscriptions;

public static class DeleteSubscriptionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Subscriptions");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromQuery] string? confirmName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        CancellationToken cancellationToken)
    {
        IResult? confirmation = ServiceBusEndpointHelpers.RequireConfirmation(confirmName, subscriptionName, "subscription");
        if (confirmation != null)
            return confirmation;

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.DeleteSubscriptionAsync(connection, topicName, subscriptionName, cancellationToken));
    }
}
