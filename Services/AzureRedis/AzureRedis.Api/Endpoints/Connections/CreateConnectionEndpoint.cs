using AZRedis.Application.Configuration;
using AZRedis.Application.Services;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AzureRedis.Api.Endpoints.Connections;

public static class CreateConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("connections", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status409Conflict)
            .WithTags("Redis Connections");
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] RedisConnectionUpsertRequest request,
        [FromServices] IOptionsMonitor<RedisOptions> options,
        [FromServices] IRedisConnectionStore store,
        CancellationToken cancellationToken)
    {
        string validation = AZRedisEndpointHelpers.ValidateConnectionRequest(request);
        if (!string.IsNullOrWhiteSpace(validation))
            return ApiResults.BadRequest(validation);

        bool existsInSettings = options.CurrentValue.Connections
            .Any(conn => conn.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
        if (existsInSettings)
            return ApiResults.Conflict($"Connection '{request.Name}' already exists in appsettings.");

        var addResult = await store.AddAsync(request.ToConfig(), cancellationToken);
        if (addResult.IsFailed &&
            addResult.Errors.Any(error => error.Message == "Connection already exists."))
        {
            return ApiResults.Conflict($"Connection '{request.Name}' already exists in connections.json.");
        }

        return ApiResults.FromResult(addResult);
    }
}
