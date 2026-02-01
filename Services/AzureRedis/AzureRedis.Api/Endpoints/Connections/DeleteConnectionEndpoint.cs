using AZRedis.Application.Services;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Connections;

public static class DeleteConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("connections/{connectionName}", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .WithTags("Redis Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IRedisConnectionStore store,
        CancellationToken cancellationToken)
    {
        var existingResult = await store.GetAsync(connectionName, cancellationToken);
        if (existingResult.IsFailed &&
            existingResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.NotFound();
        }
        if (existingResult.IsFailed)
            return ApiResults.FromResult(existingResult);

        var deleteResult = await store.DeleteAsync(connectionName, cancellationToken);
        if (deleteResult.IsFailed &&
            deleteResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.NotFound();
        }

        return ApiResults.FromResult(deleteResult);
    }
}
