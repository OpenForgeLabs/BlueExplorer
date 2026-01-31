using PurpleExplorer.Core.Configuration;

namespace PurpleExplorer.Api.Services;

public interface IConnectionStore
{
    Task<IReadOnlyList<ServiceBusConnectionConfig>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceBusConnectionConfig?> GetAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default);
    Task DeleteAsync(string name, CancellationToken cancellationToken = default);
}
