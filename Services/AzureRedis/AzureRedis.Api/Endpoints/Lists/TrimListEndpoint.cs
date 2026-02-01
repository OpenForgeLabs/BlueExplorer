using AZRedis.Application.Interfaces;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Lists;

public static class TrimListEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/lists/{key}/trim", HandleAsync)
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .WithTags("Redis Lists");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] ListTrimRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.TrimListAsync(connection, key, request.Start, request.Stop, db, cancellationToken));
    }
}
