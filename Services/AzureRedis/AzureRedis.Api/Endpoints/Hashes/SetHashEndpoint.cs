using AZRedis.Application.Interfaces;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Hashes;

public static class SetHashEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/hashes/{key}", HandleAsync)
            .Produces<ApiResponse<long>>(StatusCodes.Status200OK)
            .WithTags("Redis Hashes");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] HashSetRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.SetHashAsync(connection, key, request.Entries, db, cancellationToken));
    }
}
