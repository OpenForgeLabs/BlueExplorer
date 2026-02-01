namespace AzureRedis.Api.Contracts;

public class ListPushRequest
{
    public List<string> Values { get; set; } = [];
    public bool LeftPush { get; set; }
}
