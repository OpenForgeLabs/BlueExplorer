using AZRedis.Application.Interfaces;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Connections;

public static class GetConnectionHealthEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("connections/{connectionName}/health", HandleAsync)
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .WithTags("Redis Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisServerService serverService,
        CancellationToken cancellationToken)
    {
        var connectionResult = await AZRedisEndpointHelpers.ResolveConnection(connectionProvider, connectionName, cancellationToken);
        if (connectionResult.IsFailed)
            return ApiResults.Ok(false);

        var infoResult = await serverService.GetInfoAsync(connectionResult.Value, cancellationToken);
        return ApiResults.Ok(infoResult.IsSuccess);
    }
}
