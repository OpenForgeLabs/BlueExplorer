using Commons.Api;
using Commons.Models;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Topics;

public static class GetTopicsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/topics", HandleAsync)
            .Produces<ApiResponse<PagedResult<TopicInfo>>>(StatusCodes.Status200OK)
            .WithTags("ServiceBus Topics");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
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
            connection => topicService.GetTopicsAsync(connection, pageSize, continuationToken, cancellationToken));
    }
}
