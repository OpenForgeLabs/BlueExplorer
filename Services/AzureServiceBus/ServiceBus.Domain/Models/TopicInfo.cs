using Azure.Messaging.ServiceBus.Administration;

namespace ServiceBus.Domain.Models;

public class TopicInfo
{
    public TopicInfo(TopicRuntimeProperties runtimeProperties)
    {
        Name = runtimeProperties.Name;
    }

    public TopicInfo(TopicProperties properties)
    {
        Name = properties.Name;
    }

    public string Name { get; set; }
    public List<SubscriptionInfo> Subscriptions { get; } = [];
}
