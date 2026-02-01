namespace AZRedis.Domain.Models;

public sealed record RedisStreamEntry(
    string Id,
    IReadOnlyDictionary<string, string> Values);
