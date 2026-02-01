using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.ZSets;

public static class AddZSetEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/zsets/{key}/add", HandleAsync)
            .Produces<ApiResponse<long>>(StatusCodes.Status200OK)
            .WithTags("Redis ZSets");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] ZSetAddRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        var entries = request.Entries
            .Select(e => new RedisZSetEntry(e.Member, e.Score))
            .ToList();

        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.AddZSetAsync(connection, key, entries, db, cancellationToken));
    }
}
