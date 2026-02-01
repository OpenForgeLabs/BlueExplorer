namespace BlueExplorer.ServiceBus.Api.Endpoints;

public static class ServiceBusEndpointExtensions
{
    public static void MapServiceBusEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("api/connections")
            .WithTags("ServiceBus");

        Connections.GetConnectionsEndpoint.Map(group);
        Connections.GetConnectionEndpoint.Map(group);
        Connections.GetConnectionHealthEndpoint.Map(group);
        Connections.CreateConnectionEndpoint.Map(group);
        Connections.UpdateConnectionEndpoint.Map(group);
        Connections.DeleteConnectionEndpoint.Map(group);
        Connections.GetNamespaceInfoEndpoint.Map(group);

        Topics.GetTopicsEndpoint.Map(group);
        Topics.CreateTopicEndpoint.Map(group);
        Topics.DeleteTopicEndpoint.Map(group);

        Subscriptions.GetSubscriptionsEndpoint.Map(group);
        Subscriptions.CreateSubscriptionEndpoint.Map(group);
        Subscriptions.DeleteSubscriptionEndpoint.Map(group);

        Queues.GetQueuesEndpoint.Map(group);
        Queues.CreateQueueEndpoint.Map(group);
        Queues.DeleteQueueEndpoint.Map(group);

        Messages.GetSubscriptionMessagesEndpoint.Map(group);
        Messages.GetSubscriptionDlqMessagesEndpoint.Map(group);
        Messages.GetQueueMessagesEndpoint.Map(group);
        Messages.GetQueueDlqMessagesEndpoint.Map(group);

        Messages.SendTopicMessageEndpoint.Map(group);
        Messages.SendQueueMessageEndpoint.Map(group);

        Messages.DeleteSubscriptionMessageEndpoint.Map(group);
        Messages.DeleteQueueMessageEndpoint.Map(group);

        Messages.DeadletterSubscriptionMessageEndpoint.Map(group);
        Messages.DeadletterQueueMessageEndpoint.Map(group);

        Messages.ResubmitSubscriptionMessageEndpoint.Map(group);
        Messages.ResubmitQueueMessageEndpoint.Map(group);

        Messages.PurgeSubscriptionMessagesEndpoint.Map(group);
        Messages.PurgeQueueMessagesEndpoint.Map(group);

        Messages.TransferSubscriptionDlqEndpoint.Map(group);
        Messages.TransferQueueDlqEndpoint.Map(group);
    }
}
