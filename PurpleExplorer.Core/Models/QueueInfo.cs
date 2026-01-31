using Azure.Messaging.ServiceBus.Administration;

namespace PurpleExplorer.Core.Models;

public class QueueInfo
{
    public QueueInfo(QueueRuntimeProperties runtimeInfo)
    {
        Name = runtimeInfo.Name;
        MessageCount = runtimeInfo.ActiveMessageCount;
        DlqCount = runtimeInfo.DeadLetterMessageCount;
    }

    public string Name { get; set; }
    public long MessageCount { get; set; }
    public long DlqCount { get; set; }
}
