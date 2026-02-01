using Commons.Models;
using FluentResults;
using ServiceBus.Domain.Models;

namespace ServiceBus.Application.Interfaces;

public interface IQueueService
{
    Task<Result<PagedResult<QueueInfo>>> GetQueuesAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
    Task<Result<QueueInfo>> GetQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
    Task<Result<PagedResult<MessageDto>>> GetMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken);
    Task<Result> SendMessageAsync(ServiceBusConnection connection, string queueName, string content, CancellationToken cancellationToken);
    Task<Result> DeleteMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken);
    Task<Result> DeadletterMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        string messageId,
        CancellationToken cancellationToken);
    Task<Result> ResubmitDlqMessageAsync(
        ServiceBusConnection connection,
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken);
    Task<Result<long>> PurgeMessagesAsync(
        ServiceBusConnection connection,
        string queueName,
        bool isDlq,
        CancellationToken cancellationToken);
    Task<Result<long>> TransferDlqMessagesAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
    Task<Result> CreateQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
    Task<Result> DeleteQueueAsync(ServiceBusConnection connection, string queueName, CancellationToken cancellationToken);
}
