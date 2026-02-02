using AZRedis.Application.Configuration;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using AZRedis.Application.Interfaces;
using AZRedis.Infrastructure;

namespace AzureRedis.Api.Endpoints.Connections;

public static class TestConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("connections/test", HandleAsync)
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .WithTags("Redis Connections");
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] RedisConnectionUpsertRequest request,
        [FromServices] IRedisConnectionTester connectionTester,
        CancellationToken cancellationToken)
    {
        string validation = AZRedisEndpointHelpers.ValidateConnectionRequest(request);
        if (!string.IsNullOrWhiteSpace(validation))
            return ApiResults.BadRequest(validation);

        RedisConnectionConfig config = request.ToConfig();
        var connection = RedisConnectionMapper.ToDomain(config);
        var result = await connectionTester.TestConnectionAsync(connection, cancellationToken);
        return ApiResults.FromResult(result);
    }
}
