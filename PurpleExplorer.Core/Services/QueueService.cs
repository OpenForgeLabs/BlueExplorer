using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;
using PurpleExplorer.Core.Configuration;
using PurpleExplorer.Core.Models;
using AzureMessage = Azure.Messaging.ServiceBus.ServiceBusMessage;

namespace PurpleExplorer.Core.Services;

public class QueueService : BaseService, IQueueService
{
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public QueueService(IOptionsMonitor<ServiceBusOptions> options)
    {
        _options = options;
    }

    public async Task<PagedResult<QueueInfo>> GetQueuesAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        int size = pageSize <= 0 ? _options.CurrentValue.Query.QueueListFetchCount : pageSize;

        AsyncPageable<QueueRuntimeProperties> allQueues = client.GetQueuesRuntimePropertiesAsync(cancellationToken);
        await foreach (Page<QueueRuntimeProperties> page in allQueues.AsPages(continuationToken, size))
        {
            List<QueueInfo> items = page.Values.Select(queue => new QueueInfo(queue)).ToList();
            return new PagedResult<QueueInfo>(items, page.ContinuationToken);
        }

        return new PagedResult<QueueInfo>(Array.Empty<QueueInfo>(), null);
    }

    public async Task<QueueInfo> GetQueueAsync(
        ServiceBusConnection connection,
        string queueName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        Response<QueueRuntimeProperties> runtimeInfo =
            await client.GetQueueRuntimePropertiesAsync(queueName, cancellationToken);
        return new QueueInfo(runtimeInfo.Value);
    }

    public async Task<PagedResult<MessageDto>> GetMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken)
    {
        string path = isDlq ? $"{queueName}/$DeadLetterQueue" : queueName;

        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            path,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        int size = pageSize <= 0 ? _options.CurrentValue.Query.QueueMessageFetchCount : pageSize;
        IReadOnlyList<ServiceBusReceivedMessage> messages =
            await receiver.PeekMessagesAsync(size, fromSequenceNumber, cancellationToken);

        List<MessageDto> items = messages.Select(msg => new MessageDto(msg, isDlq)).ToList();
        long? nextSequence = messages.Count > 0 ? messages[^1].SequenceNumber + 1 : null;
        return new PagedResult<MessageDto>(items, nextSequence?.ToString());
    }

    public async Task SendMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string content,
        CancellationToken cancellationToken)
    {
        var message = new AzureMessage(content);
        await SendMessageAsync(connection, queueName, message, cancellationToken);
    }

    private async Task SendMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        AzureMessage message,
        CancellationToken cancellationToken)
    {
        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusSender sender = client.CreateSender(queueName);
        await sender.SendMessageAsync(message, cancellationToken);
    }

    public async Task DeleteMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        string path = isDlq ? $"{queueName}/$DeadLetterQueue" : queueName;

        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            path,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        while (true)
        {
            IReadOnlyList<ServiceBusReceivedMessage> messages =
                await receiver.ReceiveMessagesAsync(
                    _options.CurrentValue.Query.QueueMessageFetchCount,
                    cancellationToken: cancellationToken);
            if (messages.Count == 0) break;

            ServiceBusReceivedMessage? foundMessage =
                messages.FirstOrDefault(m => m.MessageId.Equals(messageId, StringComparison.OrdinalIgnoreCase));
            if (foundMessage != null)
            {
                await receiver.CompleteMessageAsync(foundMessage, cancellationToken);
                break;
            }
        }
    }

    public async Task DeadletterMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        CancellationToken cancellationToken)
    {
        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            queueName,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        while (true)
        {
            IReadOnlyList<ServiceBusReceivedMessage> messages =
                await receiver.ReceiveMessagesAsync(
                    _options.CurrentValue.Query.QueueMessageFetchCount,
                    cancellationToken: cancellationToken);
            if (messages.Count == 0) break;

            ServiceBusReceivedMessage? foundMessage =
                messages.FirstOrDefault(m => m.MessageId.Equals(messageId, StringComparison.OrdinalIgnoreCase));
            if (foundMessage != null)
            {
                await receiver.DeadLetterMessageAsync(foundMessage, cancellationToken: cancellationToken);
                break;
            }
        }
    }

    public async Task ResubmitDlqMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken)
    {
        ServiceBusReceivedMessage azureMessage =
            await PeekDlqMessageBySequenceNumber(connection, queueName, sequenceNumber, cancellationToken);
        var clonedMessage = new AzureMessage(azureMessage);

        await SendMessageAsync(connection, queueName, clonedMessage, cancellationToken);
        await DeleteMessageAsync(connection, queueName, azureMessage.MessageId, true, cancellationToken);
    }

    public async Task<long> PurgeMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        string path = isDlq ? $"{queueName}/$DeadLetterQueue" : queueName;

        long purgedCount = 0;
        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            path,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            });

        TimeSpan operationTimeout = TimeSpan.FromSeconds(5);
        while (true)
        {
            IReadOnlyList<ServiceBusReceivedMessage> messages = await receiver.ReceiveMessagesAsync(
                _options.CurrentValue.Query.QueueMessageFetchCount,
                operationTimeout,
                cancellationToken);
            if (messages.Count == 0) break;

            purgedCount += messages.Count;
        }

        return purgedCount;
    }

    public async Task<long> TransferDlqMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        CancellationToken cancellationToken)
    {
        var path = $"{queueName}/$DeadLetterQueue";

        long transferredCount = 0;
        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            path,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            });
        ServiceBusSender sender = client.CreateSender(queueName);

        TimeSpan operationTimeout = TimeSpan.FromSeconds(5);
        while (true)
        {
            IReadOnlyList<ServiceBusReceivedMessage> messages = await receiver.ReceiveMessagesAsync(
                _options.CurrentValue.Query.QueueMessageFetchCount,
                operationTimeout,
                cancellationToken);
            if (messages.Count == 0) break;

            IEnumerable<AzureMessage> messagesToSend = messages.Select(m => new AzureMessage(m));
            await sender.SendMessagesAsync(messagesToSend, cancellationToken);

            transferredCount += messages.Count;
        }

        return transferredCount;
    }

    public async Task CreateQueueAsync(
        ServiceBusConnection connection,
        string queueName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        await client.CreateQueueAsync(queueName, cancellationToken);
    }

    public async Task DeleteQueueAsync(
        ServiceBusConnection connection,
        string queueName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        await client.DeleteQueueAsync(queueName, cancellationToken);
    }

    private async Task<ServiceBusReceivedMessage> PeekDlqMessageBySequenceNumber(
        ServiceBusConnection connection,
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken)
    {
        var deadletterPath = $"{queueName}/$DeadLetterQueue";

        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            deadletterPath,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });
        ServiceBusReceivedMessage? azureMessage = await receiver.PeekMessageAsync(sequenceNumber, cancellationToken);

        if (azureMessage == null)
            throw new InvalidOperationException($"DLQ message with sequence {sequenceNumber} was not found.");

        return azureMessage;
    }
}
