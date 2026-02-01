using Commons.Models;
using FluentResults;
using ServiceBus.Domain.Models;

namespace ServiceBus.Application.Interfaces;

public interface IServiceBusConnectionProvider
{
    Result<IReadOnlyList<ConnectionInfo>> GetConnections();
    Task<Result<ServiceBusConnection>> GetConnectionAsync(string name, CancellationToken cancellationToken = default);
}
