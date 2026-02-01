namespace AzureRedis.Api.Contracts;

public class ZSetAddRequest
{
    public List<ZSetEntryRequest> Entries { get; set; } = [];
}

public class ZSetEntryRequest
{
    public string Member { get; set; } = string.Empty;
    public double Score { get; set; }
}
