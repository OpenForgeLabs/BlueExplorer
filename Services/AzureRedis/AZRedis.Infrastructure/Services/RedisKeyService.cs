using System.Net;
using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Results;
using FluentResults;
using StackExchange.Redis;

namespace AZRedis.Infrastructure.Services;

public class RedisKeyService : IRedisKeyService
{
    private readonly RedisConnectionManager _connectionManager = new();

    public async Task<Result<RedisKeyScanResult>> ScanKeysAsync(
        RedisConnection connection,
        string? pattern,
        int pageSize,
        long cursor,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            int page = pageSize <= 0 ? 100 : pageSize;
            RedisResult result = await db.ExecuteAsync(
                "SCAN",
                cursor.ToString(),
                "MATCH",
                pattern ?? "*",
                "COUNT",
                page.ToString());

            var inner = (RedisResult[])result!;
            long nextCursor = long.Parse((string)inner[0]!);
            RedisResult[] keys = (RedisResult[])inner[1]!;
            List<string> list = keys.Select(x => (string)x!).ToList();
            return Result.Ok(new RedisKeyScanResult(list, nextCursor));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to scan keys.", new[] { ex.Message }));
        }
    }

    public async Task<Result<RedisKeyScanResultWithInfo>> ScanKeysWithInfoAsync(
        RedisConnection connection,
        string? pattern,
        int pageSize,
        long cursor,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            int page = pageSize <= 0 ? 100 : pageSize;
            RedisResult result = await db.ExecuteAsync(
                "SCAN",
                cursor.ToString(),
                "MATCH",
                pattern ?? "*",
                "COUNT",
                page.ToString());

            var inner = (RedisResult[])result!;
            long nextCursor = long.Parse((string)inner[0]!);
            RedisResult[] keys = (RedisResult[])inner[1]!;
            List<string> keyList = keys.Select(x => (string)x!).ToList();

            List<Task<RedisKeyInfo>> infoTasks = keyList.Select(async key =>
            {
                RedisType type = await db.KeyTypeAsync(key);
                TimeSpan? ttl = await db.KeyTimeToLiveAsync(key);
                long? ttlSeconds = ttl.HasValue ? (long)ttl.Value.TotalSeconds : null;
                return new RedisKeyInfo(key, type.ToString(), ttlSeconds);
            }).ToList();

            RedisKeyInfo[] info = await Task.WhenAll(infoTasks);
            return Result.Ok(new RedisKeyScanResultWithInfo(info, nextCursor));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to scan keys with info.", new[] { ex.Message }));
        }
    }

    public async Task<Result<RedisKeyInfo>> GetKeyInfoAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisType type = await db.KeyTypeAsync(key);
            TimeSpan? ttl = await db.KeyTimeToLiveAsync(key);
            long? ttlSeconds = ttl.HasValue ? (long)ttl.Value.TotalSeconds : null;
            return Result.Ok(new RedisKeyInfo(key, type.ToString(), ttlSeconds));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get key info.", new[] { ex.Message }));
        }
    }

    public async Task<Result<bool>> DeleteKeyAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            bool deleted = await db.KeyDeleteAsync(key);
            return Result.Ok(deleted);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to delete key.", new[] { ex.Message }));
        }
    }

    public async Task<Result<bool>> RenameKeyAsync(
        RedisConnection connection,
        string key,
        string newKey,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            bool renamed = await db.KeyRenameAsync(key, newKey);
            return Result.Ok(renamed);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to rename key.", new[] { ex.Message }));
        }
    }

    public async Task<Result<bool>> SetExpiryAsync(
        RedisConnection connection,
        string key,
        TimeSpan? expiry,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            bool updated = await db.KeyExpireAsync(key, expiry);
            return Result.Ok(updated);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to update key expiry.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> FlushDatabaseAsync(
        RedisConnection connection,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            ConnectionMultiplexer multiplexer = await _connectionManager.GetAsync(connection);
            EndPoint endpoint = multiplexer.GetEndPoints().First();
            IServer server = multiplexer.GetServer(endpoint);

            int dbIndex = database ?? connection.Database ?? 0;
            await server.FlushDatabaseAsync(dbIndex);
            return Result.Ok(1L);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to flush database.", new[] { ex.Message }));
        }
    }

    private async Task<IDatabase> GetDatabaseAsync(RedisConnection connection, int? database)
    {
        ConnectionMultiplexer multiplexer = await _connectionManager.GetAsync(connection);
        int dbIndex = database ?? connection.Database ?? 0;
        return multiplexer.GetDatabase(dbIndex);
    }
}
