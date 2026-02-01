using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Connections;

public static class GetConnectionsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("connections", HandleAsync)
            .Produces<ApiResponse<IReadOnlyList<RedisConnectionInfo>>>(StatusCodes.Status200OK)
            .WithTags("Redis Connections");
    }

    private static IResult HandleAsync([FromServices] IRedisConnectionProvider connectionProvider)
    {
        var result = connectionProvider.GetConnections();
        return ApiResults.FromResult(result);
    }
}
