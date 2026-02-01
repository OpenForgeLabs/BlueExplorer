using AZRedis.Application.Services;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Connections;

public static class UpdateConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("connections/{connectionName}", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .WithTags("Redis Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromBody] RedisConnectionUpsertRequest request,
        [FromServices] IRedisConnectionStore store,
        CancellationToken cancellationToken)
    {
        if (!connectionName.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            return ApiResults.BadRequest("Connection name mismatch.");

        string validation = AZRedisEndpointHelpers.ValidateConnectionRequest(request);
        if (!string.IsNullOrWhiteSpace(validation))
            return ApiResults.BadRequest(validation);

        var updateResult = await store.UpdateAsync(request.ToConfig(), cancellationToken);
        if (updateResult.IsFailed &&
            updateResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.NotFound();
        }

        return ApiResults.FromResult(updateResult);
    }
}
