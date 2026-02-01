using AZRedis.Application.Interfaces;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Strings;

public static class SetStringEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/strings/{key}", HandleAsync)
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .WithTags("Redis Strings");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] StringSetRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        TimeSpan? expiry = request.ExpirySeconds.HasValue ? TimeSpan.FromSeconds(request.ExpirySeconds.Value) : null;
        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.SetStringAsync(connection, key, request.Value, expiry, db, cancellationToken));
    }
}
