using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PurpleExplorer.Core.Models;

namespace PurpleExplorer.Core.Services;

public interface ITopicService
{
    Task<NamespaceInfo> GetNamespaceInfoAsync(ServiceBusConnection connection, CancellationToken cancellationToken);
    Task<PagedResult<TopicInfo>> GetTopicsAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
    Task<PagedResult<SubscriptionInfo>> GetSubscriptionsAsync(
        ServiceBusConnection connection,
        string topicName,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken);
    Task<SubscriptionInfo> GetSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
    Task<PagedResult<MessageDto>> GetSubscriptionMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken);
    Task SendMessageAsync(ServiceBusConnection connection, string topicName, string content, CancellationToken cancellationToken);
    Task DeleteMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken);
    Task DeadletterMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        string messageId,
        CancellationToken cancellationToken);
    Task ResubmitDlqMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        long sequenceNumber,
        CancellationToken cancellationToken);
    Task<long> PurgeMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        CancellationToken cancellationToken);
    Task<long> TransferDlqMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
    Task CreateTopicAsync(ServiceBusConnection connection, string topicName, CancellationToken cancellationToken);
    Task DeleteTopicAsync(ServiceBusConnection connection, string topicName, CancellationToken cancellationToken);
    Task CreateSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
    Task DeleteSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken);
}
