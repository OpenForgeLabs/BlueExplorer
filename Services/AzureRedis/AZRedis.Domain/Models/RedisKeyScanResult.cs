namespace AZRedis.Domain.Models;

public sealed record RedisKeyScanResult(
    IReadOnlyList<string> Keys,
    long Cursor);
