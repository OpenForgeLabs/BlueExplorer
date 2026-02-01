using FluentResults;
using ServiceBus.Application.Configuration;

namespace ServiceBus.Application.Services;

public interface IConnectionStore
{
    Task<Result<IReadOnlyList<ServiceBusConnectionConfig>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ServiceBusConnectionConfig>> GetAsync(string name, CancellationToken cancellationToken = default);
    Task<Result> AddAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(ServiceBusConnectionConfig connection, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(string name, CancellationToken cancellationToken = default);
}
