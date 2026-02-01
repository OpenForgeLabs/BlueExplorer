namespace AZRedis.Application.Configuration;

public class RedisOptions
{
    public List<RedisConnectionConfig> Connections { get; set; } = [];
    public RedisQuerySettings Query { get; set; } = new();
}

public class RedisConnectionConfig
{
    public string Name { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public bool UseTls { get; set; }
    public int? Database { get; set; }
}

public class RedisQuerySettings
{
    public int DefaultPageSize { get; set; } = 100;
}
