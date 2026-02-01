using AZRedis.Application.Configuration;
using AZRedis.Application.Interfaces;
using AZRedis.Domain.Models;
using Commons.Results;
using FluentResults;
using Microsoft.Extensions.Options;

namespace AZRedis.Application.Services;

public class ApiRedisConnectionProvider : IRedisConnectionProvider
{
    private readonly IRedisConnectionStore _store;
    private readonly IOptionsMonitor<RedisOptions> _options;

    public ApiRedisConnectionProvider(
        IRedisConnectionStore store,
        IOptionsMonitor<RedisOptions> options)
    {
        _store = store;
        _options = options;
    }

    public Result<IReadOnlyList<RedisConnectionInfo>> GetConnections()
    {
        try
        {
            List<RedisConnectionInfo> items = _options.CurrentValue.Connections
                .Select(config => new RedisConnectionInfo(
                    config.Name,
                    config.UseTls,
                    config.Database,
                    false,
                    "appsettings"))
                .ToList();

            Result<IReadOnlyList<RedisConnectionConfig>> storedResult = _store.GetAllAsync().GetAwaiter().GetResult();
            if (storedResult.IsFailed)
            {
                IEnumerable<string> reasons = storedResult.Reasons.Select(reason => reason.Message);
                return Result.Fail(new HandledFail("Failed to load connections.", reasons));
            }

            IReadOnlyList<RedisConnectionConfig> stored = storedResult.Value;
            items.AddRange(stored.Select(config => new RedisConnectionInfo(
                config.Name,
                config.UseTls,
                config.Database,
                true,
                "connections.json")));

            return Result.Ok((IReadOnlyList<RedisConnectionInfo>)items);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to load connections.", new[] { ex.Message }));
        }
    }

    public async Task<Result<RedisConnection>> GetConnectionAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            RedisConnectionConfig? config = await GetConfigAsync(name, cancellationToken);
            if (config == null)
                return Result.Fail(new HandledFail("Connection not found.", new[] { $"No connection configured with name '{name}'." }));

            return Result.Ok(ToDomain(config));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to resolve connection.", new[] { ex.Message }));
        }
    }

    private async Task<RedisConnectionConfig?> GetConfigAsync(string name, CancellationToken cancellationToken)
    {
        RedisConnectionConfig? config = _options.CurrentValue.Connections
            .FirstOrDefault(conn => conn.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (config != null)
            return config;

        Result<RedisConnectionConfig> storeResult = await _store.GetAsync(name, cancellationToken);
        if (storeResult.IsSuccess)
            return storeResult.Value;
        if (storeResult.Errors.Any(error => error.Message == "Connection not found."))
            return null;

        throw new InvalidOperationException(storeResult.Errors.First().Message);
    }

    private static RedisConnection ToDomain(RedisConnectionConfig config)
    {
        return new RedisConnection(
            config.Name,
            config.ConnectionString,
            config.Host,
            config.Port,
            config.Password,
            config.UseTls,
            config.Database);
    }
}
