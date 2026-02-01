using System.Net;
using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Results;
using FluentResults;
using StackExchange.Redis;

namespace AZRedis.Infrastructure.Services;

public class RedisServerService : IRedisServerService
{
    private readonly RedisConnectionManager _connectionManager = new();

    public async Task<Result<RedisServerInfo>> GetInfoAsync(RedisConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            ConnectionMultiplexer multiplexer = await _connectionManager.GetAsync(connection);
            IServer server = GetServer(multiplexer);
            IGrouping<string, KeyValuePair<string, string>>[] sections = await server.InfoAsync();

            Dictionary<string, IReadOnlyDictionary<string, string>> map = sections
                .ToDictionary(
                    section => section.Key,
                    section => (IReadOnlyDictionary<string, string>)section.ToDictionary(x => x.Key, x => x.Value));

            return Result.Ok(new RedisServerInfo(map));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get Redis server info.", new[] { ex.Message }));
        }
    }

    public async Task<Result<RedisRawInfo>> GetClusterInfoAsync(RedisConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            ConnectionMultiplexer multiplexer = await _connectionManager.GetAsync(connection);
            IServer server = GetServer(multiplexer);
            RedisResult result = await server.ExecuteAsync("CLUSTER", "INFO");
            return Result.Ok(new RedisRawInfo(result.ToString()));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get Redis cluster info.", new[] { ex.Message }));
        }
    }

    public async Task<Result<RedisRawInfo>> GetClusterNodesAsync(RedisConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            ConnectionMultiplexer multiplexer = await _connectionManager.GetAsync(connection);
            IServer server = GetServer(multiplexer);
            RedisResult result = await server.ExecuteAsync("CLUSTER", "NODES");
            return Result.Ok(new RedisRawInfo(result.ToString()));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get Redis cluster nodes.", new[] { ex.Message }));
        }
    }

    public async Task<Result<RedisRawInfo>> GetClusterSlotsAsync(RedisConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            ConnectionMultiplexer multiplexer = await _connectionManager.GetAsync(connection);
            IServer server = GetServer(multiplexer);
            RedisResult result = await server.ExecuteAsync("CLUSTER", "SLOTS");
            return Result.Ok(new RedisRawInfo(result.ToString()));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get Redis cluster slots.", new[] { ex.Message }));
        }
    }

    private static IServer GetServer(ConnectionMultiplexer multiplexer)
    {
        EndPoint endpoint = multiplexer.GetEndPoints().First();
        return multiplexer.GetServer(endpoint);
    }
}
