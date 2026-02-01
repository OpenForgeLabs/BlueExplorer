using BlueExplorer.ServiceBus.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Subscriptions;

public static class CreateSubscriptionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/topics/{topicName}/subscriptions", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("ServiceBus Subscriptions");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string topicName,
        [FromBody] CreateSubscriptionRequest request,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResults.BadRequest("Name is required.");

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.CreateSubscriptionAsync(connection, topicName, request.Name, cancellationToken));
    }
}
