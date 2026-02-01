using AZRedis.Application.Interfaces;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Sets;

public static class AddSetEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/sets/{key}/add", HandleAsync)
            .Produces<ApiResponse<long>>(StatusCodes.Status200OK)
            .WithTags("Redis Sets");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] SetUpdateRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.AddSetAsync(connection, key, request.Members, db, cancellationToken));
    }
}
