namespace AzureRedis.Api.Contracts;

public class StringSetRequest
{
    public string Value { get; set; } = string.Empty;
    public int? ExpirySeconds { get; set; }
}
