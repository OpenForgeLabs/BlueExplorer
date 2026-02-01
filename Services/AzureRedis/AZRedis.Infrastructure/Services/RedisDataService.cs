using AZRedis.Application;
using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Results;
using FluentResults;
using StackExchange.Redis;

namespace AZRedis.Infrastructure.Services;

public class RedisDataService : IRedisDataService
{
    private readonly RedisConnectionManager _connectionManager = new();

    public async Task<Result<string?>> GetStringAsync(RedisConnection connection, string key, int? database, CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue value = await db.StringGetAsync(key);
            return Result.Ok(value.HasValue ? value.ToString() : null);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get string value.", new[] { ex.Message }));
        }
    }

    public async Task<Result<bool>> SetStringAsync(
        RedisConnection connection,
        string key,
        string value,
        TimeSpan? expiry,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            
            TimeSpan? expiration = expiry ?? TimeSpan.Zero;

            // This parameter is to preserve or not an existent ttl
            bool keepTtl = !expiry.HasValue;

            bool result = await db.StringSetAsync(
                key,
                value,
                expiry: expiration,
                keepTtl: keepTtl,
                flags: CommandFlags.None
            );

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail(
                new HandledFail(
                    "Failed to set string value.",
                    new[] { ex.Message }
                )
            );
        }
    }

    public async Task<Result<IReadOnlyDictionary<string, string>>> GetHashAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            HashEntry[] entries = await db.HashGetAllAsync(key);
            return Result.Ok((IReadOnlyDictionary<string, string>)entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString()));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get hash.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> SetHashAsync(
        RedisConnection connection,
        string key,
        IReadOnlyDictionary<string, string> entries,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            HashEntry[] hashEntries = entries.Select(pair => new HashEntry(pair.Key, pair.Value)).ToArray();
            await db.HashSetAsync(key, hashEntries);
            return Result.Ok((long)hashEntries.Length);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to set hash.", new[] { ex.Message }));
        }
    }

    public async Task<Result<IReadOnlyList<string>>> GetListAsync(
        RedisConnection connection,
        string key,
        long start,
        long stop,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue[] values = await db.ListRangeAsync(key, start, stop);
            return Result.Ok((IReadOnlyList<string>)values.Select(x => x.ToString()).ToList());
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get list.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> PushListAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> values,
        bool leftPush,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue[] listValues = values.Select(x => (RedisValue)x).ToArray();
            long length = leftPush
                ? await db.ListLeftPushAsync(key, listValues)
                : await db.ListRightPushAsync(key, listValues);
            return Result.Ok(length);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to push list values.", new[] { ex.Message }));
        }
    }

    public async Task<Result<bool>> TrimListAsync(
        RedisConnection connection,
        string key,
        long start,
        long stop,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            await db.ListTrimAsync(key, start, stop);
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to trim list.", new[] { ex.Message }));
        }
    }

    public async Task<Result<IReadOnlyList<string>>> GetSetAsync(
        RedisConnection connection,
        string key,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue[] values = await db.SetMembersAsync(key);
            return Result.Ok((IReadOnlyList<string>)values.Select(x => x.ToString()).ToList());
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get set.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> AddSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> members,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue[] values = members.Select(x => (RedisValue)x).ToArray();
            long added = await db.SetAddAsync(key, values);
            return Result.Ok(added);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to add set members.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> RemoveSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> members,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue[] values = members.Select(x => (RedisValue)x).ToArray();
            long removed = await db.SetRemoveAsync(key, values);
            return Result.Ok(removed);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to remove set members.", new[] { ex.Message }));
        }
    }

    public async Task<Result<IReadOnlyList<RedisZSetEntry>>> GetZSetAsync(
        RedisConnection connection,
        string key,
        long start,
        long stop,
        bool withScores,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            if (!withScores)
            {
                RedisValue[] values = await db.SortedSetRangeByRankAsync(key, start, stop);
                IReadOnlyList<RedisZSetEntry> list = values.Select(v => new RedisZSetEntry(v.ToString(), 0)).ToList();
                return Result.Ok(list);
            }

            SortedSetEntry[] entries = await db.SortedSetRangeByRankWithScoresAsync(key, start, stop);
            IReadOnlyList<RedisZSetEntry> result = entries.Select(e => new RedisZSetEntry(e.Element.ToString(), e.Score)).ToList();
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get sorted set.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> AddZSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<RedisZSetEntry> entries,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            SortedSetEntry[] items = entries
                .Select(e => new SortedSetEntry(e.Member, e.Score))
                .ToArray();
            long added = await db.SortedSetAddAsync(key, items);
            return Result.Ok(added);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to add sorted set members.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> RemoveZSetAsync(
        RedisConnection connection,
        string key,
        IReadOnlyList<string> members,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue[] values = members.Select(x => (RedisValue)x).ToArray();
            long removed = await db.SortedSetRemoveAsync(key, values);
            return Result.Ok(removed);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to remove sorted set members.", new[] { ex.Message }));
        }
    }

    public async Task<Result<IReadOnlyList<RedisStreamEntry>>> GetStreamAsync(
        RedisConnection connection,
        string key,
        string start,
        string end,
        int? count,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            RedisValue startValue = string.IsNullOrWhiteSpace(start) ? "-" : start;
            RedisValue endValue = string.IsNullOrWhiteSpace(end) ? "+" : end;
            StreamEntry[] entries = await db.StreamRangeAsync(key, startValue, endValue, count);
            IReadOnlyList<RedisStreamEntry> result = entries.Select(e => new RedisStreamEntry(
                e.Id.HasValue ? e.Id.ToString() : string.Empty,
                e.Values.ToDictionary(v => v.Name.ToString(), v => v.Value.ToString()))).ToList();
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get stream.", new[] { ex.Message }));
        }
    }

    public async Task<Result<string>> AddStreamEntryAsync(
        RedisConnection connection,
        string key,
        IReadOnlyDictionary<string, string> values,
        string? id,
        int? database,
        CancellationToken cancellationToken)
    {
        try
        {
            IDatabase db = await GetDatabaseAsync(connection, database);
            NameValueEntry[] entries = values.Select(pair => new NameValueEntry(pair.Key, pair.Value)).ToArray();
            RedisValue entryId = string.IsNullOrWhiteSpace(id) ? "*" : id;
            RedisValue result = await db.StreamAddAsync(key, entries, entryId);
            return Result.Ok(result.ToString());
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to add stream entry.", new[] { ex.Message }));
        }
    }

    private async Task<IDatabase> GetDatabaseAsync(RedisConnection connection, int? database)
    {
        ConnectionMultiplexer multiplexer = await _connectionManager.GetAsync(connection);
        int dbIndex = database ?? connection.Database ?? 0;
        return multiplexer.GetDatabase(dbIndex);
    }
}
