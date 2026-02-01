namespace AzureRedis.Api.Contracts;

public class PublishRequest
{
    public string Channel { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
