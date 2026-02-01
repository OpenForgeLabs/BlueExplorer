using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.ZSets;

public static class GetZSetEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/zsets/{key}", HandleAsync)
            .Produces<ApiResponse<IReadOnlyList<RedisZSetEntry>>>(StatusCodes.Status200OK)
            .WithTags("Redis ZSets");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] long start = 0,
        [FromQuery] long stop = -1,
        [FromQuery] bool withScores = true,
        [FromQuery] int? db = null,
        CancellationToken cancellationToken = default)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.GetZSetAsync(connection, key, start, stop, withScores, db, cancellationToken));
    }
}
