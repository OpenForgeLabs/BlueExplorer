namespace AzureRedis.Api.Contracts;

public class StreamAddRequest
{
    public Dictionary<string, string> Values { get; set; } = new();
    public string? Id { get; set; }
}
