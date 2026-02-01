namespace AzureRedis.Api.Contracts;

public class ZSetRemoveRequest
{
    public List<string> Members { get; set; } = [];
}
