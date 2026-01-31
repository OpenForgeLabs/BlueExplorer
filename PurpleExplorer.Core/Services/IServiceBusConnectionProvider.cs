using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PurpleExplorer.Core.Models;

namespace PurpleExplorer.Core.Services;

public interface IServiceBusConnectionProvider
{
    IReadOnlyList<ConnectionInfo> GetConnections();
    Task<ServiceBusConnection> GetConnectionAsync(string name, CancellationToken cancellationToken = default);
}
