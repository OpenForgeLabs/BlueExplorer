using AZRedis.Application.Interfaces;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Streams;

public static class AddStreamEntryEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/streams/{key}", HandleAsync)
            .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
            .WithTags("Redis Streams");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] StreamAddRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.AddStreamEntryAsync(connection, key, request.Values, request.Id, db, cancellationToken));
    }
}
