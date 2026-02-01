namespace AZRedis.Domain.Models;

public sealed record RedisServerInfo(
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Sections);
