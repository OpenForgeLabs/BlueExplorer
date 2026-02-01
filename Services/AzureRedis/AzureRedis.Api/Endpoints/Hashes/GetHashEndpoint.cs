using AZRedis.Application.Interfaces;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Hashes;

public static class GetHashEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/hashes/{key}", HandleAsync)
            .Produces<ApiResponse<IReadOnlyDictionary<string, string>>>(StatusCodes.Status200OK)
            .WithTags("Redis Hashes");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.GetHashAsync(connection, key, db, cancellationToken));
    }
}
