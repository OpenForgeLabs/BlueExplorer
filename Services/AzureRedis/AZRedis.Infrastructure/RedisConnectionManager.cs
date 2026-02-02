using System.Collections.Concurrent;
using AZRedis.Domain.Models;
using StackExchange.Redis;

namespace AZRedis.Infrastructure;

public sealed class RedisConnectionManager
{
    private readonly ConcurrentDictionary<string, Lazy<Task<ConnectionMultiplexer>>> _connections = new();

    public Task<ConnectionMultiplexer> GetAsync(RedisConnection connection)
    {
        Lazy<Task<ConnectionMultiplexer>> lazy = _connections.GetOrAdd(
            connection.Name,
            _ => new Lazy<Task<ConnectionMultiplexer>>(() => ConnectAsync(connection)));

        return lazy.Value;
    }

    private static Task<ConnectionMultiplexer> ConnectAsync(RedisConnection connection)
    {
        ConfigurationOptions options = RedisClientFactory.BuildOptions(connection);
        return ConnectionMultiplexer.ConnectAsync(options);
    }
}
