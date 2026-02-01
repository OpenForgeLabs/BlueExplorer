using AZRedis.Application.Interfaces;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Lists;

public static class PushListEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/lists/{key}/push", HandleAsync)
            .Produces<ApiResponse<long>>(StatusCodes.Status200OK)
            .WithTags("Redis Lists");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] ListPushRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.PushListAsync(connection, key, request.Values, request.LeftPush, db, cancellationToken));
    }
}
