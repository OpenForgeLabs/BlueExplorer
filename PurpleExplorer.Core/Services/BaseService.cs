using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using PurpleExplorer.Core.Models;

namespace PurpleExplorer.Core.Services;

public abstract class BaseService
{
    protected ServiceBusAdministrationClient GetManagementClient(ServiceBusConnection connection)
    {
        if (connection.UseManagedIdentity)
            return new ServiceBusAdministrationClient(connection.ConnectionString, new DefaultAzureCredential());

        return new ServiceBusAdministrationClient(connection.ConnectionString);
    }

    protected ServiceBusClient GetServiceBusClient(ServiceBusConnection connection)
    {
        if (connection.UseManagedIdentity)
            return new ServiceBusClient(connection.ConnectionString, new DefaultAzureCredential());

        return new ServiceBusClient(connection.ConnectionString);
    }

    protected ServiceBusReceiver CreateReceiver(
        ServiceBusConnection connection,
        string path,
        ServiceBusReceiveMode receiveMode)
    {
        ServiceBusClient client = GetServiceBusClient(connection);
        return client.CreateReceiver(
            path,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = receiveMode
            });
    }

    protected ServiceBusSender CreateSender(ServiceBusConnection connection, string path)
    {
        ServiceBusClient client = GetServiceBusClient(connection);
        return client.CreateSender(path);
    }
}
