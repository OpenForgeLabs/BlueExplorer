using AZRedis.Application.Interfaces;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Keys;

public static class FlushDatabaseEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/keys/flush", HandleAsync)
            .Produces<ApiResponse<long>>(StatusCodes.Status200OK)
            .WithTags("Redis Keys");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromQuery] string? confirmName,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisKeyService keyService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        string expected = db?.ToString() ?? "default";
        IResult? confirmation = AZRedisEndpointHelpers.RequireConfirmation(confirmName, expected, "database");
        if (confirmation != null)
            return confirmation;

        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => keyService.FlushDatabaseAsync(connection, db, cancellationToken));
    }
}
