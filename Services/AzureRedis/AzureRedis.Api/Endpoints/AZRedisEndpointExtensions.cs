namespace AzureRedis.Api.Endpoints;

public static class AZRedisEndpointExtensions
{
    public static void MapAZRedisEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("api/redis")
            .WithTags("AZRedis");

        Connections.GetConnectionsEndpoint.Map(group);
        Connections.GetConnectionEndpoint.Map(group);
        Connections.CreateConnectionEndpoint.Map(group);
        Connections.UpdateConnectionEndpoint.Map(group);
        Connections.DeleteConnectionEndpoint.Map(group);
        Connections.GetConnectionHealthEndpoint.Map(group);

        Keys.ScanKeysEndpoint.Map(group);
        Keys.GetKeyInfoEndpoint.Map(group);
        Keys.DeleteKeyEndpoint.Map(group);
        Keys.RenameKeyEndpoint.Map(group);
        Keys.ExpireKeyEndpoint.Map(group);
        Keys.FlushDatabaseEndpoint.Map(group);

        Strings.GetStringEndpoint.Map(group);
        Strings.SetStringEndpoint.Map(group);

        Hashes.GetHashEndpoint.Map(group);
        Hashes.SetHashEndpoint.Map(group);

        Lists.GetListEndpoint.Map(group);
        Lists.PushListEndpoint.Map(group);
        Lists.TrimListEndpoint.Map(group);

        Sets.GetSetEndpoint.Map(group);
        Sets.AddSetEndpoint.Map(group);
        Sets.RemoveSetEndpoint.Map(group);

        ZSets.GetZSetEndpoint.Map(group);
        ZSets.AddZSetEndpoint.Map(group);
        ZSets.RemoveZSetEndpoint.Map(group);

        Streams.GetStreamEndpoint.Map(group);
        Streams.AddStreamEntryEndpoint.Map(group);

        Server.GetServerInfoEndpoint.Map(group);
        Server.GetClusterInfoEndpoint.Map(group);
        Server.GetClusterNodesEndpoint.Map(group);
        Server.GetClusterSlotsEndpoint.Map(group);
    }
}
