using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Keys;

public static class GetKeyInfoEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/keys/{key}/info", HandleAsync)
            .Produces<ApiResponse<RedisKeyInfo>>(StatusCodes.Status200OK)
            .WithTags("Redis Keys");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisKeyService keyService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => keyService.GetKeyInfoAsync(connection, key, db, cancellationToken));
    }
}
