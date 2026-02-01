using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Streams;

public static class GetStreamEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/streams/{key}", HandleAsync)
            .Produces<ApiResponse<IReadOnlyList<RedisStreamEntry>>>(StatusCodes.Status200OK)
            .WithTags("Redis Streams");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] string start = "-",
        [FromQuery] string end = "+",
        [FromQuery] int? count = null,
        [FromQuery] int? db = null,
        CancellationToken cancellationToken = default)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.GetStreamAsync(connection, key, start, end, count, db, cancellationToken));
    }
}
