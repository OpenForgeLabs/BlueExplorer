using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Keys;

public static class ScanKeysWithInfoEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/keys/enriched", HandleAsync)
            .Produces<ApiResponse<RedisKeyScanResultWithInfo>>(StatusCodes.Status200OK)
            .WithTags("Redis Keys");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisKeyService keyService,
        [FromQuery] string? pattern,
        [FromQuery] int pageSize = 100,
        [FromQuery] long cursor = 0,
        [FromQuery] int? db = null,
        CancellationToken cancellationToken = default)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => keyService.ScanKeysWithInfoAsync(connection, pattern, pageSize, cursor, db, cancellationToken));
    }
}
