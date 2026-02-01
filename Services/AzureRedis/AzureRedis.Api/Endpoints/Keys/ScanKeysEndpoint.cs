using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Keys;

public static class ScanKeysEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/keys", HandleAsync)
            .Produces<ApiResponse<RedisKeyScanResult>>(StatusCodes.Status200OK)
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
            connection => keyService.ScanKeysAsync(connection, pattern, pageSize, cursor, db, cancellationToken));
    }
}
