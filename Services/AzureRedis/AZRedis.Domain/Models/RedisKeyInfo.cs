namespace AZRedis.Domain.Models;

public sealed record RedisKeyInfo(
    string Key,
    string Type,
    long? TtlSeconds);
