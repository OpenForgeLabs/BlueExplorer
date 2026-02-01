using AZRedis.Application.Interfaces;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Strings;

public static class GetStringEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/strings/{key}", HandleAsync)
            .Produces<ApiResponse<string?>>(StatusCodes.Status200OK)
            .WithTags("Redis Strings");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.GetStringAsync(connection, key, db, cancellationToken));
    }
}
