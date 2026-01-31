using Azure.Messaging.ServiceBus.Administration;

namespace PurpleExplorer.Core.Models;

public class SubscriptionInfo
{
    public SubscriptionInfo(SubscriptionRuntimeProperties runtimeInfo)
    {
        Name = runtimeInfo.SubscriptionName;
        MessageCount = runtimeInfo.ActiveMessageCount;
        DlqCount = runtimeInfo.DeadLetterMessageCount;
    }

    public string Name { get; set; }
    public long MessageCount { get; set; }
    public long DlqCount { get; set; }
}
