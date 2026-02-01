namespace AzureRedis.Api.Contracts;

public class ExpireKeyRequest
{
    public int? TtlSeconds { get; set; }
}
