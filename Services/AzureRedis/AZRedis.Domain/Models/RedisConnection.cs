namespace AZRedis.Domain.Models;

public sealed record RedisConnection(
    string Name,
    string? ConnectionString,
    string Host,
    int Port,
    string? Password,
    bool UseTls,
    int? Database);
