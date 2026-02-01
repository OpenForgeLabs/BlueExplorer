using Azure.Messaging.ServiceBus.Administration;

namespace ServiceBus.Domain.Models;

public class NamespaceInfo
{
    public NamespaceInfo(NamespaceProperties properties)
    {
        Name = properties.Name;
        CreatedTime = properties.CreatedTime;
    }

    public string Name { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
