using System.Text.Json;
using System.Linq;
using PurpleExplorer.Core.Configuration;

namespace PurpleExplorer.Api.Services;

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

    public async Task<IReadOnlyList<ServiceBusConnectionConfig>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return await ReadFileAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ServiceBusConnectionConfig?> GetAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ServiceBusConnectionConfig> connections = await GetAllAsync(cancellationToken);
        return connections.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task AddAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<ServiceBusConnectionConfig> connections = (await ReadFileAsync(cancellationToken)).ToList();
            if (connections.Any(c => c.Name.Equals(connection.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Connection '{connection.Name}' already exists.");

            connections.Add(connection);
            await WriteFileAsync(connections, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<ServiceBusConnectionConfig> connections = (await ReadFileAsync(cancellationToken)).ToList();
            int index = connections.FindIndex(c => c.Name.Equals(connection.Name, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
                throw new KeyNotFoundException($"Connection '{connection.Name}' was not found.");

            connections[index] = connection;
            await WriteFileAsync(connections, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAsync(string name, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            List<ServiceBusConnectionConfig> connections = (await ReadFileAsync(cancellationToken)).ToList();
            connections.RemoveAll(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            await WriteFileAsync(connections, cancellationToken);
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
