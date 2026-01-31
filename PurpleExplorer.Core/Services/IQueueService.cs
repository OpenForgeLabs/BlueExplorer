using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PurpleExplorer.Core.Models;

namespace PurpleExplorer.Core.Services;

public interface IQueueService
{
    Task<PagedResult<QueueInfo>> GetQueuesAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
    Task<QueueInfo> GetQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
    Task<PagedResult<MessageDto>> GetMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken);
    Task SendMessageAsync(ServiceBusConnection connection, string queueName, string content, CancellationToken cancellationToken);
    Task DeleteMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken);
    Task DeadletterMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        CancellationToken cancellationToken);
    Task ResubmitDlqMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken);
    Task<long> PurgeMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        CancellationToken cancellationToken);
    Task<long> TransferDlqMessagesAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
    Task CreateQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
    Task DeleteQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
}
