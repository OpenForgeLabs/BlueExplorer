using AZRedis.Domain.Models;
using FluentResults;

namespace AZRedis.Application.Interfaces;

public interface IRedisServerService
{
    Task<Result<RedisServerInfo>> GetInfoAsync(RedisConnection connection, CancellationToken cancellationToken);
    Task<Result<RedisRawInfo>> GetClusterInfoAsync(RedisConnection connection, CancellationToken cancellationToken);
    Task<Result<RedisRawInfo>> GetClusterNodesAsync(RedisConnection connection, CancellationToken cancellationToken);
    Task<Result<RedisRawInfo>> GetClusterSlotsAsync(RedisConnection connection, CancellationToken cancellationToken);
}
