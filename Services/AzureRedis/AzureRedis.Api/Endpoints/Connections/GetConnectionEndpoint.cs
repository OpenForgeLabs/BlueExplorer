using AZRedis.Application.Configuration;
using AZRedis.Application.Services;
using AzureRedis.Api.Contracts;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AzureRedis.Api.Endpoints.Connections;

public static class GetConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("connections/{connectionName}", HandleAsync)
            .Produces<ApiResponse<RedisConnectionUpsertRequest>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .WithTags("Redis Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromServices] IOptionsMonitor<RedisOptions> options,
        [FromServices] IRedisConnectionStore store,
        CancellationToken cancellationToken)
    {
        var storeResult = await store.GetAsync(connectionName, cancellationToken);
        if (storeResult.IsSuccess)
        {
            RedisConnectionConfig config = storeResult.Value;
            return ApiResults.Ok(new RedisConnectionUpsertRequest
            {
                Name = config.Name,
                ConnectionString = config.ConnectionString,
                Host = config.Host,
                Port = config.Port,
                Password = config.Password,
                UseTls = config.UseTls,
                Database = config.Database
            });
        }

        if (storeResult.IsFailed &&
            !storeResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.FromResult(storeResult);
        }

        RedisConnectionConfig? fromSettings = options.CurrentValue.Connections
            .FirstOrDefault(conn => conn.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
        if (fromSettings == null)
            return ApiResults.NotFound();

        return ApiResults.Ok(new RedisConnectionUpsertRequest
        {
            Name = fromSettings.Name,
            ConnectionString = null,
            Host = fromSettings.Host,
            Port = fromSettings.Port,
            Password = null,
            UseTls = fromSettings.UseTls,
            Database = fromSettings.Database
        });
    }
}
