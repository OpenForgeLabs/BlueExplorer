using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Server;

public static class GetClusterNodesEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/cluster/nodes", HandleAsync)
            .Produces<ApiResponse<RedisRawInfo>>(StatusCodes.Status200OK)
            .WithTags("Redis Server");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisServerService serverService,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => serverService.GetClusterNodesAsync(connection, cancellationToken));
    }
}
