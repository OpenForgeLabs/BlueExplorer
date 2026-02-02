using AZRedis.Application.Interfaces;
using Commons.Api;
using Microsoft.AspNetCore.Mvc;

namespace AzureRedis.Api.Endpoints.Server;

public static class GetDatabaseSizeEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("{connectionName}/databases/{db}/size", HandleAsync)
            .Produces<ApiResponse<long>>(StatusCodes.Status200OK)
            .WithTags("Redis Server");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        int db,
        [FromServices] IRedisConnectionProvider connectionProvider,
        [FromServices] IRedisDataService dataService,
        CancellationToken cancellationToken)
    {
        if (db < 0)
        {
            return Results.BadRequest(ApiResults.BadRequest(
                "Invalid database index.",
                ["Database index must be zero or greater."]));
        }

        return await AZRedisEndpointHelpers.WithConnection(
            connectionProvider,
            connectionName,
            cancellationToken,
            connection => dataService.GetDatabaseSizeAsync(connection, db, cancellationToken));
    }
}
