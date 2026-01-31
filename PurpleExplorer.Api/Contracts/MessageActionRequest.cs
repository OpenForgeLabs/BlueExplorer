namespace PurpleExplorer.Api.Contracts;

public class MessageActionRequest
{
    public string MessageId { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public bool IsDlq { get; set; }
}
