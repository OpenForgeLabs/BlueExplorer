using AZRedis.Application.Configuration;

namespace AzureRedis.Api.Contracts;

public class RedisConnectionUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public bool UseTls { get; set; }
    public int? Database { get; set; }
    public string? Environment { get; set; } = "development";

    public RedisConnectionConfig ToConfig()
    {
        return new RedisConnectionConfig
        {
            Name = Name,
            ConnectionString = ConnectionString,
            Host = Host,
            Port = Port,
            Password = Password,
            UseTls = UseTls,
            Database = Database,
            Environment = Environment
        };
    }
}
