using AZRedis.Domain.Models;
using FluentResults;

namespace AZRedis.Application.Interfaces;

public interface IRedisDataService
{
    Task<Result<string?>> GetStringAsync(RedisConnection connection, string key, int? database, CancellationToken cancellationToken);
    Task<Result<bool>> SetStringAsync(
        RedisConnection connection,
        string key,
        string value,
        TimeSpan? expiry,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyDictionary<string, string>>> GetHashAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<long>> SetHashAsync(
        RedisConnection connection,
        string key,
        IReadOnlyDictionary<string, string> entries,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>>> GetListAsync(
        RedisConnection connection,
        string key,
        long start,
        long stop,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<long>> PushListAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> values,
        bool leftPush,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<bool>> TrimListAsync(
        RedisConnection connection,
        string key,
        long start,
        long stop,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>>> GetSetAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<long>> AddSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> members,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<long>> RemoveSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> members,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<RedisZSetEntry>>> GetZSetAsync(
        RedisConnection connection,
        string key,
        long start,
        long stop,
        bool withScores,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<long>> AddZSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<RedisZSetEntry> entries,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<long>> RemoveZSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> members,
        int? database,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<RedisStreamEntry>>> GetStreamAsync(
        RedisConnection connection,
        string key,
        string start,
        string end,
        int? count,
        int? database,
        CancellationToken cancellationToken);
    Task<Result<string>> AddStreamEntryAsync(
        RedisConnection connection,
        string key,
        IReadOnlyDictionary<string, string> values,
        string? id,
        int? database,
        CancellationToken cancellationToken);
}
