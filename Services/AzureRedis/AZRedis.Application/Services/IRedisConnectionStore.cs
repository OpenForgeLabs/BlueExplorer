using AZRedis.Application.Configuration;
using FluentResults;

namespace AZRedis.Application.Services;

public interface IRedisConnectionStore
{
    Task<Result<IReadOnlyList<RedisConnectionConfig>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<RedisConnectionConfig>> GetAsync(string name, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(RedisConnectionConfig config, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(RedisConnectionConfig config, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string name, CancellationToken cancellationToken = default);
}
