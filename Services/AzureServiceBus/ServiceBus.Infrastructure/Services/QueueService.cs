using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Commons.Models;
using Commons.Results;
using FluentResults;
using Microsoft.Extensions.Options;
using ServiceBus.Application.Configuration;
using ServiceBus.Application.Interfaces;
using ServiceBus.Domain.Models;
using AzureMessage = Azure.Messaging.ServiceBus.ServiceBusMessage;

namespace ServiceBus.Infrastructure.Services;

public class QueueService : BaseService, IQueueService
{
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public QueueService(IOptionsMonitor<ServiceBusOptions> options)
    {
        _options = options;
    }

    public async Task<Result<PagedResult<QueueInfo>>> GetQueuesAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            int size = pageSize <= 0 ? _options.CurrentValue.Query.QueueListFetchCount : pageSize;

            AsyncPageable<QueueRuntimeProperties> allQueues = client.GetQueuesRuntimePropertiesAsync(cancellationToken);
            await foreach (Page<QueueRuntimeProperties> page in allQueues.AsPages(continuationToken, size))
            {
                List<QueueInfo> items = page.Values.Select(queue => new QueueInfo(queue)).ToList();
                return Result.Ok(new PagedResult<QueueInfo>(items, page.ContinuationToken));
            }

            return Result.Ok(new PagedResult<QueueInfo>([], null));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get queues.", new[] { ex.Message }));
        }
    }

    public async Task<Result<QueueInfo>> GetQueueAsync(
        ServiceBusConnection connection,
        string queueName,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            QueueRuntimeProperties queue = await client.GetQueueRuntimePropertiesAsync(queueName, cancellationToken);
            return Result.Ok(new QueueInfo(queue));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get queue.", new[] { ex.Message }));
        }
    }

    public async Task<Result<PagedResult<MessageDto>>> GetMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusClient client = GetServiceBusClient(connection);
            ServiceBusReceiver receiver = CreateReceiver(connection, queueName, isDlq);
            int size = pageSize <= 0 ? _options.CurrentValue.Query.QueueMessageFetchCount : pageSize;

            IEnumerable<ServiceBusReceivedMessage> peeked =
                await receiver.PeekMessagesAsync(size, fromSequenceNumber, cancellationToken);
            List<MessageDto> items = peeked.Select(message => new MessageDto(message, isDlq)).ToList();

            string? continuationToken = items.Count > 0
                ? items.Max(message => message.SequenceNumber).ToString()
                : null;
            return Result.Ok(new PagedResult<MessageDto>(items, continuationToken));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get queue messages.", new[] { ex.Message }));
        }
    }

    public async Task<Result> SendMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string content,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusClient client = GetServiceBusClient(connection);
            ServiceBusSender sender = client.CreateSender(queueName);

            var message = new AzureMessage(content);
            await sender.SendMessageAsync(message, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to send queue message.", new[] { ex.Message }));
        }
    }

    public async Task<Result> DeleteMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, queueName, isDlq, ServiceBusReceiveMode.ReceiveAndDelete);

            ServiceBusReceivedMessage? message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            while (message != null)
            {
                if (message.MessageId == messageId)
                    return Result.Ok();

                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            }

            return Result.Fail(new HandledFail("Message not found.", new[] { "MessageId not found in queue." }));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to delete queue message.", new[] { ex.Message }));
        }
    }

    public async Task<Result> DeadletterMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, queueName, false);
            ServiceBusReceivedMessage? message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            while (message != null)
            {
                if (message.MessageId == messageId)
                {
                    await receiver.DeadLetterMessageAsync(message, cancellationToken: cancellationToken);
                    return Result.Ok();
                }

                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            }

            return Result.Fail(new HandledFail("Message not found.", new[] { "MessageId not found in queue." }));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to deadletter queue message.", new[] { ex.Message }));
        }
    }

    public async Task<Result> ResubmitDlqMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, queueName, true, ServiceBusReceiveMode.ReceiveAndDelete);
            ServiceBusSender sender = GetServiceBusClient(connection).CreateSender(queueName);

            ServiceBusReceivedMessage? message =
                await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            while (message != null)
            {
                if (message.SequenceNumber == sequenceNumber)
                {
                    var clonedMessage = new AzureMessage(message);
                    await sender.SendMessageAsync(clonedMessage, cancellationToken);
                    return Result.Ok();
                }

                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            }

            return Result.Fail(new HandledFail("Message not found.", new[] { "SequenceNumber not found in DLQ." }));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to resubmit DLQ message.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> PurgeMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, queueName, isDlq, ServiceBusReceiveMode.ReceiveAndDelete);

            long purged = 0;
            ServiceBusReceivedMessage? message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1), cancellationToken);
            while (message != null)
            {
                purged++;
                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1), cancellationToken);
            }

            return Result.Ok(purged);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to purge messages.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> TransferDlqMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, queueName, true, ServiceBusReceiveMode.ReceiveAndDelete);
            ServiceBusSender sender = GetServiceBusClient(connection).CreateSender(queueName);

            long transferred = 0;
            ServiceBusReceivedMessage? message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1), cancellationToken);
            while (message != null)
            {
                IEnumerable<ServiceBusMessage> messagesToSend = new[] { new AzureMessage(message) };
                await sender.SendMessagesAsync(messagesToSend, cancellationToken);
                transferred++;
                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1), cancellationToken);
            }

            return Result.Ok(transferred);
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to transfer DLQ messages.", new[] { ex.Message }));
        }
    }

    public async Task<Result> CreateQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            await client.CreateQueueAsync(queueName, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to create queue.", new[] { ex.Message }));
        }
    }

    public async Task<Result> DeleteQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            await client.DeleteQueueAsync(queueName, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to delete queue.", new[] { ex.Message }));
        }
    }
}
