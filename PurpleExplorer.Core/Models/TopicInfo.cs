using System.Collections.Generic;
using Azure.Messaging.ServiceBus.Administration;

namespace PurpleExplorer.Core.Models;

public class TopicInfo
{
    public TopicInfo(TopicProperties properties)
    {
        Name = properties.Name;
    }

    public string Name { get; set; }
    public List<SubscriptionInfo> Subscriptions { get; } = [];
}
