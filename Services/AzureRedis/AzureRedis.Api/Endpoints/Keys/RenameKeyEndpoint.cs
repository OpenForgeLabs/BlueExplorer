using AZRedis.Application.Interfaces;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Keys;

public static class RenameKeyEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("{connectionName}/keys/{key}/rename", HandleAsync)
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("Redis Keys");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        string key,
        [FromBody] RenameKeyRequest request,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisKeyService keyService,
        [FromQuery] int? db,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NewKey))
            return ApiResults.BadRequest("NewKey is required.");

        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => keyService.RenameKeyAsync(connection, key, request.NewKey, db, cancellationToken));
    }
}
