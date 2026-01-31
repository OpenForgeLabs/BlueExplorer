using System.Collections.Generic;

namespace PurpleExplorer.Web.Models;

public class TopicInfo
{
    public string Name { get; set; } = string.Empty;
    public List<SubscriptionInfo> Subscriptions { get; set; } = [];
}
