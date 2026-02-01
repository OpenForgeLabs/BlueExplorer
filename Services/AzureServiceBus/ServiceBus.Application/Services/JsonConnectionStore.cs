using System.Text.Json;
using Commons.Results;
using FluentResults;
using Microsoft.AspNetCore.Hosting;
using ServiceBus.Application.Configuration;

namespace ServiceBus.Application.Services;

public class JsonConnectionStore : IConnectionStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public JsonConnectionStore(IWebHostEnvironment environment)
    {
        string dataDir = Path.Combine(environment.ContentRootPath, "data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "connections.json");
    }

    public async Task<Result<IReadOnlyList<ServiceBusConnectionConfig>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            IReadOnlyList<ServiceBusConnectionConfig> connections = await ReadFileAsync(cancellationToken);
            return Result.Ok(connections);
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

    public async Task<Result<ServiceBusConnectionConfig>> GetAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyList<ServiceBusConnectionConfig>> result = await GetAllAsync(cancellationToken);
        if (result.IsFailed)
            return Result.Fail(result.Errors);

        ServiceBusConnectionConfig? connection =
            result.Value.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (connection == null)
        {
            return Result.Fail(new HandledFail(
                "Connection not found.",
                new[] { $"No connection configured with name '{name}'." }));
        }

        return Result.Ok(connection);
    }

    public async Task<Result> AddAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<ServiceBusConnectionConfig> connections = (await ReadFileAsync(cancellationToken)).ToList();
            if (connections.Any(c => c.Name.Equals(connection.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Result.Fail(new HandledFail(
                    "Connection already exists.",
                    new[] { $"Connection '{connection.Name}' already exists." }));
            }

            connections.Add(connection);
            await WriteFileAsync(connections, cancellationToken);
            return Result.Ok();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Result> UpdateAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<ServiceBusConnectionConfig> connections = (await ReadFileAsync(cancellationToken)).ToList();
            int index = connections.FindIndex(c => c.Name.Equals(connection.Name, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return Result.Fail(new HandledFail(
                    "Connection not found.",
                    new[] { $"Connection '{connection.Name}' was not found." }));
            }

            connections[index] = connection;
            await WriteFileAsync(connections, cancellationToken);
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
            List<ServiceBusConnectionConfig> connections = (await ReadFileAsync(cancellationToken)).ToList();
            int removed = connections.RemoveAll(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (removed == 0)
            {
                return Result.Fail(new HandledFail(
                    "Connection not found.",
                    new[] { $"Connection '{name}' was not found." }));
            }

            await WriteFileAsync(connections, cancellationToken);
            return Result.Ok();
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IReadOnlyList<ServiceBusConnectionConfig>> ReadFileAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
            return Array.Empty<ServiceBusConnectionConfig>();

        await using FileStream stream = File.OpenRead(_filePath);
        List<ServiceBusConnectionConfig>? data =
            await JsonSerializer.DeserializeAsync<List<ServiceBusConnectionConfig>>(stream, _jsonOptions, cancellationToken);
        return data ?? [];
    }

    private async Task WriteFileAsync(
        List<ServiceBusConnectionConfig> connections,
        CancellationToken cancellationToken)
    {
        await using FileStream stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, connections, _jsonOptions, cancellationToken);
    }
}
