using System;
using System.Collections.Generic;

namespace PurpleExplorer.Web.Models;

public class MessageDto
{
    public string MessageId { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public long Size { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public int DeliveryCount { get; set; }
    public long SequenceNumber { get; set; }
    public TimeSpan TimeToLive { get; set; }
    public DateTimeOffset EnqueueTimeUtc { get; set; }
    public string DeadLetterReason { get; set; } = string.Empty;
    public bool IsDlq { get; set; }
    public Dictionary<string, string?> ApplicationProperties { get; set; } = [];
}
