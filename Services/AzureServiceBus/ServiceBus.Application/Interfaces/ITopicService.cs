using Commons.Models;
using FluentResults;
using ServiceBus.Domain.Models;

namespace ServiceBus.Application.Interfaces;

public interface ITopicService
{
    Task<Result<NamespaceInfo>> GetNamespaceInfoAsync(ServiceBusConnection connection, CancellationToken cancellationToken);
    Task<Result<PagedResult<TopicInfo>>> GetTopicsAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
    Task<Result<PagedResult<SubscriptionInfo>>> GetSubscriptionsAsync(
        ServiceBusConnection connection,
        string topicName,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
    Task<Result<SubscriptionInfo>> GetSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
    Task<Result<PagedResult<MessageDto>>> GetSubscriptionMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken);
    Task<Result> SendMessageAsync(ServiceBusConnection connection, string topicName, string content, CancellationToken cancellationToken);
    Task<Result> DeleteMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken);
    Task<Result> DeadletterMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        string messageId,
        CancellationToken cancellationToken);
    Task<Result> ResubmitDlqMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        long sequenceNumber,
        CancellationToken cancellationToken);
    Task<Result<long>> PurgeMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        CancellationToken cancellationToken);
    Task<Result<long>> TransferDlqMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
    Task<Result> CreateTopicAsync(ServiceBusConnection connection, string topicName, CancellationToken cancellationToken);
    Task<Result> DeleteTopicAsync(ServiceBusConnection connection, string topicName, CancellationToken cancellationToken);
    Task<Result> CreateSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
    Task<Result> DeleteSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
}
