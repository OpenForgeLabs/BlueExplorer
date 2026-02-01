using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace ServiceBus.Infrastructure.Services;

public abstract class BaseService
{
    protected ServiceBusAdministrationClient GetManagementClient(Domain.Models.ServiceBusConnection connection)
    {
        if (connection.UseManagedIdentity)
            return new ServiceBusAdministrationClient(connection.ConnectionString, new DefaultAzureCredential());

        return new ServiceBusAdministrationClient(connection.ConnectionString);
    }

    protected ServiceBusClient GetServiceBusClient(Domain.Models.ServiceBusConnection connection)
    {
        if (connection.UseManagedIdentity)
            return new ServiceBusClient(connection.ConnectionString, new DefaultAzureCredential());

        return new ServiceBusClient(connection.ConnectionString);
    }

    protected ServiceBusReceiver CreateReceiver(
        Domain.Models.ServiceBusConnection connection,
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

    protected ServiceBusReceiver CreateReceiver(
        Domain.Models.ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        ServiceBusReceiveMode receiveMode = ServiceBusReceiveMode.PeekLock)
    {
        ServiceBusClient client = GetServiceBusClient(connection);
        return client.CreateReceiver(
            queueName,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = receiveMode,
                SubQueue = isDlq ? SubQueue.DeadLetter : SubQueue.None
            });
    }

    protected ServiceBusReceiver CreateReceiver(
        Domain.Models.ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        ServiceBusReceiveMode receiveMode = ServiceBusReceiveMode.PeekLock)
    {
        ServiceBusClient client = GetServiceBusClient(connection);
        return client.CreateReceiver(
            topicName,
            subscriptionName,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = receiveMode,
                SubQueue = isDlq ? SubQueue.DeadLetter : SubQueue.None
            });
    }

    protected ServiceBusSender CreateSender(Domain.Models.ServiceBusConnection connection, string path)
    {
        ServiceBusClient client = GetServiceBusClient(connection);
        return client.CreateSender(path);
    }
}
