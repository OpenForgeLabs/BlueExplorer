namespace AzureRedis.Api.Contracts;

public class RenameKeyRequest
{
    public string NewKey { get; set; } = string.Empty;
}
