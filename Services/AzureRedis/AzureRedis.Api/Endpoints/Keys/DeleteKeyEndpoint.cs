using AZRedis.Application.Interfaces;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Keys;

public static class DeleteKeyEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("{connectionName}/keys/{key}", HandleAsync)
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .WithTags("Redis Keys");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromQuery] string? confirmName,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisKeyService keyService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        IResult? confirmation = AZRedisEndpointHelpers.RequireConfirmation(confirmName, key, "key");
        if (confirmation != null)
            return confirmation;

        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => keyService.DeleteKeyAsync(connection, key, db, cancellationToken));
    }
}
