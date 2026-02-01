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

public class TopicService : BaseService, ITopicService
{
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public TopicService(IOptionsMonitor<ServiceBusOptions> options)
    {
        _options = options;
    }

    public async Task<Result<NamespaceInfo>> GetNamespaceInfoAsync(ServiceBusConnection connection, CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            NamespaceProperties props = await client.GetNamespacePropertiesAsync(cancellationToken);
            return Result.Ok(new NamespaceInfo(props));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get namespace info.", new[] { ex.Message }));
        }
    }

    public async Task<Result<PagedResult<TopicInfo>>> GetTopicsAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            int size = pageSize <= 0 ? _options.CurrentValue.Query.TopicListFetchCount : pageSize;

            AsyncPageable<TopicRuntimeProperties> allTopics = client.GetTopicsRuntimePropertiesAsync(cancellationToken);
            await foreach (Page<TopicRuntimeProperties> page in allTopics.AsPages(continuationToken, size))
            {
                List<TopicInfo> items = page.Values.Select(topic => new TopicInfo(topic)).ToList();
                return Result.Ok(new PagedResult<TopicInfo>(items, page.ContinuationToken));
            }

            return Result.Ok(new PagedResult<TopicInfo>([], null));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get topics.", new[] { ex.Message }));
        }
    }

    public async Task<Result<PagedResult<SubscriptionInfo>>> GetSubscriptionsAsync(
        ServiceBusConnection connection,
        string topicName,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            int size = pageSize <= 0 ? _options.CurrentValue.Query.TopicListFetchCount : pageSize;

            AsyncPageable<SubscriptionRuntimeProperties> allSubscriptions =
                client.GetSubscriptionsRuntimePropertiesAsync(topicName, cancellationToken);
            await foreach (Page<SubscriptionRuntimeProperties> page in allSubscriptions.AsPages(continuationToken, size))
            {
                List<SubscriptionInfo> items = page.Values.Select(sub => new SubscriptionInfo(sub)).ToList();
                return Result.Ok(new PagedResult<SubscriptionInfo>(items, page.ContinuationToken));
            }

            return Result.Ok(new PagedResult<SubscriptionInfo>([], null));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get subscriptions.", new[] { ex.Message }));
        }
    }

    public async Task<Result<SubscriptionInfo>> GetSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            SubscriptionRuntimeProperties subscription =
                await client.GetSubscriptionRuntimePropertiesAsync(topicName, subscriptionName, cancellationToken);
            return Result.Ok(new SubscriptionInfo(subscription));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to get subscription.", new[] { ex.Message }));
        }
    }

    public async Task<Result<PagedResult<MessageDto>>> GetSubscriptionMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, topicName, subscriptionName, isDlq);
            int size = pageSize <= 0 ? _options.CurrentValue.Query.TopicMessageFetchCount : pageSize;

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
            return Result.Fail(new HandledFail("Failed to get subscription messages.", new[] { ex.Message }));
        }
    }

    public async Task<Result> SendMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string content,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusClient client = GetServiceBusClient(connection);
            ServiceBusSender sender = client.CreateSender(topicName);

            var message = new AzureMessage(content);
            await sender.SendMessageAsync(message, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to send topic message.", new[] { ex.Message }));
        }
    }

    public async Task<Result> DeleteMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, topicName, subscriptionName, isDlq, ServiceBusReceiveMode.ReceiveAndDelete);

            ServiceBusReceivedMessage? message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            while (message != null)
            {
                if (message.MessageId == messageId)
                    return Result.Ok();

                message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5), cancellationToken);
            }

            return Result.Fail(new HandledFail("Message not found.", new[] { "MessageId not found in subscription." }));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to delete subscription message.", new[] { ex.Message }));
        }
    }

    public async Task<Result> DeadletterMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        string messageId,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, topicName, subscriptionName, false);
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

            return Result.Fail(new HandledFail("Message not found.", new[] { "MessageId not found in subscription." }));
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to deadletter subscription message.", new[] { ex.Message }));
        }
    }

    public async Task<Result> ResubmitDlqMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        long sequenceNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, topicName, subscriptionName, true, ServiceBusReceiveMode.ReceiveAndDelete);
            ServiceBusSender sender = GetServiceBusClient(connection).CreateSender(topicName);

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
        string topicName,
        string subscriptionName,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, topicName, subscriptionName, isDlq, ServiceBusReceiveMode.ReceiveAndDelete);

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
            return Result.Fail(new HandledFail("Failed to purge subscription messages.", new[] { ex.Message }));
        }
    }

    public async Task<Result<long>> TransferDlqMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusReceiver receiver = CreateReceiver(connection, topicName, subscriptionName, true, ServiceBusReceiveMode.ReceiveAndDelete);
            ServiceBusSender sender = GetServiceBusClient(connection).CreateSender(topicName);

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
            return Result.Fail(new HandledFail("Failed to transfer subscription DLQ messages.", new[] { ex.Message }));
        }
    }

    public async Task<Result> CreateTopicAsync(ServiceBusConnection connection, string topicName, CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            await client.CreateTopicAsync(topicName, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to create topic.", new[] { ex.Message }));
        }
    }

    public async Task<Result> DeleteTopicAsync(ServiceBusConnection connection, string topicName, CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            await client.DeleteTopicAsync(topicName, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to delete topic.", new[] { ex.Message }));
        }
    }

    public async Task<Result> CreateSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            await client.CreateSubscriptionAsync(topicName, subscriptionName, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to create subscription.", new[] { ex.Message }));
        }
    }

    public async Task<Result> DeleteSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusAdministrationClient client = GetManagementClient(connection);
            await client.DeleteSubscriptionAsync(topicName, subscriptionName, cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new HandledFail("Failed to delete subscription.", new[] { ex.Message }));
        }
    }
}
