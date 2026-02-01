namespace AZRedis.Domain.Models;

public sealed record RedisZSetEntry(
    string Member,
    double Score);
