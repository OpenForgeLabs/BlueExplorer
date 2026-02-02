using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Results;
using FluentResults;
using StackExchange.Redis;

namespace AZRedis.Infrastructure.Services;

public class RedisConnectionTester : IRedisConnectionTester
{
    public async Task<Result<bool>> TestConnectionAsync(
        RedisConnection connection,
        CancellationToken cancellationToken)
    {
        try
        {
            ConfigurationOptions options = RedisClientFactory.BuildOptions(connection);
            ConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync(options);
            await multiplexer.CloseAsync();
            await multiplexer.DisposeAsync();
            return Result.Ok(true);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to connect to Redis.", new[] { ex.Message }));
        }
    }
}
