using Commons.Api;
using Microsoft.AspNetCore.Mvc;
using ServiceBus.Application.Services;

namespace BlueExplorer.ServiceBus.Api.Endpoints.Connections;

public static class DeleteConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("{connectionName}", HandleAsync)
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .WithTags("ServiceBus Connections");
    }

    private static async Task<IResult> HandleAsync(
        string connectionName,
        [FromQuery] string? confirmName,
        [FromServices] IConnectionStore connectionStore,
        CancellationToken cancellationToken)
    {
        IResult? confirmation = ServiceBusEndpointHelpers.RequireConfirmation(confirmName, connectionName, "connection");
        if (confirmation != null)
            return confirmation;

        var existingResult = await connectionStore.GetAsync(connectionName, cancellationToken);
        if (existingResult.IsFailed &&
            existingResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.NotFound();
        }
        if (existingResult.IsFailed)
            return ApiResults.FromResult(existingResult);

        var deleteResult = await connectionStore.DeleteAsync(connectionName, cancellationToken);
        if (deleteResult.IsFailed &&
            deleteResult.Errors.Any(error => error.Message == "Connection not found."))
        {
            return ApiResults.NotFound();
        }

        return ApiResults.FromResult(deleteResult);
    }
}
