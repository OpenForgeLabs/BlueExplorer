namespace AzureRedis.Api.Contracts;

public class RedisConnectionSummary
{
    public string Name { get; set; } = string.Empty;
    public bool UseTls { get; set; }
    public int? Database { get; set; }
    public bool IsEditable { get; set; }
    public string Source { get; set; } = string.Empty;
}
