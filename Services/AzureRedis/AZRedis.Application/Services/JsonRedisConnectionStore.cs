using System.Text.Json;
using AZRedis.Application.Configuration;
using Commons.Results;
using FluentResults;
using Microsoft.AspNetCore.Hosting;

namespace AZRedis.Application.Services;

public class JsonRedisConnectionStore : IRedisConnectionStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public JsonRedisConnectionStore(IWebHostEnvironment environment)
    {
        string dataDir = Path.Combine(environment.ContentRootPath, "data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "redis-connections.json");
    }

    public async Task<Result<IReadOnlyList<RedisConnectionConfig>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            IReadOnlyList<RedisConnectionConfig> configs = await ReadFileAsync(cancellationToken);
            return Result.Ok(configs);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to read connections.", new[] { ex.Message }));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result<RedisConnectionConfig>> GetAsync(string name, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<RedisConnectionConfig> configs = await ReadFileAsync(cancellationToken);
            RedisConnectionConfig? config = configs
                .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (config == null)
            {
                return Result.Fail(new HandledFail(
                    "Connection not found.",
                    new[] { $"No connection configured with name '{name}'." }));
            }

            return Result.Ok(config);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result> AddAsync(RedisConnectionConfig config, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<RedisConnectionConfig> configs = await ReadFileAsync(cancellationToken);
            if (configs.Any(x => x.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Fail(new HandledFail(
                    "Connection already exists.",
                    new[] { $"Connection '{config.Name}' already exists." }));
            }

            configs.Add(config);
            await WriteFileAsync(configs, cancellationToken);
            return Result.Ok();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result> UpdateAsync(RedisConnectionConfig config, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<RedisConnectionConfig> configs = await ReadFileAsync(cancellationToken);
            int index = configs.FindIndex(x => x.Name.Equals(config.Name, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return Result.Fail(new HandledFail(
                    "Connection not found.",
                    new[] { $"Connection '{config.Name}' was not found." }));
            }
            configs[index] = config;
            await WriteFileAsync(configs, cancellationToken);
            return Result.Ok();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result> DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<RedisConnectionConfig> configs = await ReadFileAsync(cancellationToken);
            int removed = configs.RemoveAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (removed == 0)
            {
                return Result.Fail(new HandledFail(
                    "Connection not found.",
                    new[] { $"Connection '{name}' was not found." }));
            }
            await WriteFileAsync(configs, cancellationToken);
            return Result.Ok();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<RedisConnectionConfig>> ReadFileAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return new List<RedisConnectionConfig>();

        string content = await File.ReadAllTextAsync(_filePath, cancellationToken);
        return JsonSerializer.Deserialize<List<RedisConnectionConfig>>(content, _jsonOptions) ?? new List<RedisConnectionConfig>();
    }

    private Task WriteFileAsync(List<RedisConnectionConfig> configs, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(configs, _jsonOptions);
        return File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }
}
