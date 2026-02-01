namespace AzureRedis.Api.Contracts;

public class ListTrimRequest
{
    public long Start { get; set; }
    public long Stop { get; set; }
}
