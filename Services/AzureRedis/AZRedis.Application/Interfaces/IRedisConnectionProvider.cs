using AZRedis.Domain.Models;
using FluentResults;

namespace AZRedis.Application.Interfaces;

public interface IRedisConnectionProvider
{
    Task<Result<RedisConnection>> GetConnectionAsync(string name, CancellationToken cancellationToken = default);
    Result<IReadOnlyList<RedisConnectionInfo>> GetConnections();
}
