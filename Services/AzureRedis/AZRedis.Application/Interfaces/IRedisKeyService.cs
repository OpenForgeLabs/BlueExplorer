using AZRedis.Domain.Models;
using FluentResults;

namespace AZRedis.Application.Interfaces;

public interface IRedisKeyService
{
    Task<Result<RedisKeyScanResult>> ScanKeysAsync(
        RedisConnection connection,
        string? pattern,
        int pageSize,
        long cursor,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<RedisKeyScanResultWithInfo>> ScanKeysWithInfoAsync(
        RedisConnection connection,
        string? pattern,
        int pageSize,
        long cursor,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<RedisKeyInfo>> GetKeyInfoAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<bool>> DeleteKeyAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<bool>> RenameKeyAsync(
        RedisConnection connection,
        string key,
        string newKey,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<bool>> SetExpiryAsync(
        RedisConnection connection,
        string key,
        TimeSpan? expiry,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<long>> FlushDatabaseAsync(
        RedisConnection connection,
        int? database,
        CancellationToken cancellationToken);
}
