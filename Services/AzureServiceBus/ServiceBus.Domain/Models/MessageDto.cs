using Azure.Messaging.ServiceBus;

namespace ServiceBus.Domain.Models;

public class MessageDto
{
    public MessageDto(ServiceBusReceivedMessage azureMessage, bool isDlq)
    {
        Content = azureMessage.Body != null ? azureMessage.Body.ToString() : string.Empty;
        MessageId = azureMessage.MessageId ?? string.Empty;
        CorrelationId = azureMessage.CorrelationId ?? string.Empty;
        DeliveryCount = azureMessage.DeliveryCount;
        ContentType = azureMessage.ContentType ?? string.Empty;
        Label = azureMessage.Subject ?? string.Empty;
        SequenceNumber = azureMessage.SequenceNumber;
        Size = azureMessage.Body != null ? azureMessage.Body.ToArray().Length : 0;
        TimeToLive = azureMessage.TimeToLive;
        IsDlq = isDlq;
        EnqueueTimeUtc = azureMessage.EnqueuedTime;
        ApplicationProperties = azureMessage.ApplicationProperties
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString());
        DeadLetterReason = (azureMessage.ApplicationProperties.TryGetValue("DeadLetterReason", out object? property)
            ? property?.ToString()
            : string.Empty) ?? string.Empty;
    }

    public string MessageId { get; set; }
    public string ContentType { get; set; }
    public string Content { get; set; }
    public string Label { get; set; }
    public long Size { get; set; }
    public string CorrelationId { get; set; }
    public int DeliveryCount { get; set; }
    public long SequenceNumber { get; set; }
    public TimeSpan TimeToLive { get; set; }
    public DateTimeOffset EnqueueTimeUtc { get; set; }
    public string DeadLetterReason { get; set; }
    public bool IsDlq { get; }
    public Dictionary<string, string?> ApplicationProperties { get; set; }
}
