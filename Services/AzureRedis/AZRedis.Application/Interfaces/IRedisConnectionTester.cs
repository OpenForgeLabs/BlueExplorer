using AZRedis.Domain.Models;
using FluentResults;

namespace AZRedis.Application.Interfaces;

public interface IRedisConnectionTester
{
    Task<Result<bool>> TestConnectionAsync(
        RedisConnection connection,
        CancellationToken cancellationToken);
}
