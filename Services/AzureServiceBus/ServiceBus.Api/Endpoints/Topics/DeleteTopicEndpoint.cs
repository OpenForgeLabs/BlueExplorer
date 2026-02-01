using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Topics;

public static class DeleteTopicEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("{connectionName}/topics/{topicName}", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Topics");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string topicName,
        [FromQuery] string? confirmName,
        [FromServices] IServiceBusConnectionProvider connectionProvider,
        [FromServices] ITopicService topicService,
        CancellationToken cancellationToken)
    {
        IResult? confirmation = ServiceBusEndpointHelpers.RequireConfirmation(confirmName, topicName, "topic");
        if (confirmation != null)
            return confirmation;

        return await ServiceBusEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => topicService.DeleteTopicAsync(connection, topicName, cancellationToken));
    }
}
