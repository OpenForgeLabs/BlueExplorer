namespace AzureRedis.Api.Contracts;

public class HashSetRequest
{
    public Dictionary<string, string> Entries { get; set; } = new();
}
