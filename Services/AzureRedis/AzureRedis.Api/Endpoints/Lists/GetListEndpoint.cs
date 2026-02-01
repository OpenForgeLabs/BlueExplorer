using AZRedis.Application.Interfaces;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Lists;

public static class GetListEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/lists/{key}", HandleAsync)
            .Produces<ApiResponse<IReadOnlyList<string>>>(StatusCodes.Status200OK)
            .WithTags("Redis Lists");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] long start = 0,
        [FromQuery] long stop = -1,
        [FromQuery] int? db = null,
        CancellationToken cancellationToken = default)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.GetListAsync(connection, key, start, stop, db, cancellationToken));
    }
}
