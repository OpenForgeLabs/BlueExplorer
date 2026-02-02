namespace AZRedis.Domain.Models;

public sealed record RedisKeyScanResultWithInfo(
    IReadOnlyList<RedisKeyInfo> Keys,
    long Cursor);
