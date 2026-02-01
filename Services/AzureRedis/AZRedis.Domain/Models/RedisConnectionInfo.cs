namespace AZRedis.Domain.Models;

public sealed record RedisConnectionInfo(
    string Name,
    bool UseTls,
    int? Database,
    bool IsEditable,
    string Source);
