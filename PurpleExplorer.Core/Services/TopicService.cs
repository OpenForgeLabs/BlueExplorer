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

public class TopicService : BaseService, ITopicService
{
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public TopicService(IOptionsMonitor<ServiceBusOptions> options)
    {
        _options = options;
    }

    public async Task<NamespaceInfo> GetNamespaceInfoAsync(
        ServiceBusConnection connection,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        Response<NamespaceProperties> result = await client.GetNamespacePropertiesAsync(cancellationToken);
        return new NamespaceInfo(result.Value);
    }

    public async Task<PagedResult<TopicInfo>> GetTopicsAsync(
        ServiceBusConnection connection,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        int size = pageSize <= 0 ? _options.CurrentValue.Query.TopicListFetchCount : pageSize;
        AsyncPageable<TopicProperties> allTopics = client.GetTopicsAsync(cancellationToken);

        await foreach (Page<TopicProperties> page in allTopics.AsPages(continuationToken, size))
        {
            List<TopicInfo> items = page.Values.Select(topic => new TopicInfo(topic)).ToList();
            return new PagedResult<TopicInfo>(items, page.ContinuationToken);
        }

        return new PagedResult<TopicInfo>(Array.Empty<TopicInfo>(), null);
    }

    public async Task<PagedResult<SubscriptionInfo>> GetSubscriptionsAsync(
        ServiceBusConnection connection,
        string topicName,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        int size = pageSize <= 0 ? _options.CurrentValue.Query.TopicMessageFetchCount : pageSize;
        AsyncPageable<SubscriptionRuntimeProperties> subscriptions =
            client.GetSubscriptionsRuntimePropertiesAsync(topicName, cancellationToken);

        await foreach (Page<SubscriptionRuntimeProperties> page in subscriptions.AsPages(continuationToken, size))
        {
            List<SubscriptionInfo> items = page.Values.Select(sub => new SubscriptionInfo(sub)).ToList();
            return new PagedResult<SubscriptionInfo>(items, page.ContinuationToken);
        }

        return new PagedResult<SubscriptionInfo>(Array.Empty<SubscriptionInfo>(), null);
    }

    public async Task<SubscriptionInfo> GetSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        Response<SubscriptionRuntimeProperties> runtimeInfo =
            await client.GetSubscriptionRuntimePropertiesAsync(topicName, subscriptionName, cancellationToken);

        return new SubscriptionInfo(runtimeInfo.Value);
    }

    public async Task<PagedResult<MessageDto>> GetSubscriptionMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        int pageSize,
        long? fromSequenceNumber,
        CancellationToken cancellationToken)
    {
        string path = isDlq
            ? $"{topicName}/Subscriptions/{subscriptionName}/$DeadLetterQueue"
            : $"{topicName}/Subscriptions/{subscriptionName}";

        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            path,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

        int size = pageSize <= 0 ? _options.CurrentValue.Query.TopicMessageFetchCount : pageSize;
        IReadOnlyList<ServiceBusReceivedMessage> receivedMessages =
            await receiver.PeekMessagesAsync(size, fromSequenceNumber, cancellationToken);

        List<MessageDto> items = receivedMessages.Select(message => new MessageDto(message, isDlq)).ToList();
        long? nextSequence = receivedMessages.Count > 0 ? receivedMessages[^1].SequenceNumber + 1 : null;
        return new PagedResult<MessageDto>(items, nextSequence?.ToString());
    }

    public async Task SendMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string content,
        CancellationToken cancellationToken)
    {
        var message = new AzureMessage(content);
        await SendMessageAsync(connection, topicName, message, cancellationToken);
    }

    private async Task SendMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        AzureMessage message,
        CancellationToken cancellationToken)
    {
        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusSender sender = client.CreateSender(topicName);
        await sender.SendMessageAsync(message, cancellationToken);
    }

    public async Task DeleteMessageAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        string path = $"{topicName}/Subscriptions/{subscriptionName}";
        path = isDlq ? $"{path}/$DeadLetterQueue" : path;

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
                    _options.CurrentValue.Query.TopicMessageFetchCount,
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
        string topicName,
        string subscriptionName,
        string messageId,
        CancellationToken cancellationToken)
    {
        string path = $"{topicName}/Subscriptions/{subscriptionName}";

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
                    _options.CurrentValue.Query.TopicMessageFetchCount,
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
        string topicName,
        string subscriptionName,
        long sequenceNumber,
        CancellationToken cancellationToken)
    {
        ServiceBusReceivedMessage azureMessage = await PeekDlqMessageBySequenceNumber(
            connection,
            topicName,
            subscriptionName,
            sequenceNumber,
            cancellationToken);
        var clonedMessage = new AzureMessage(azureMessage);

        await SendMessageAsync(connection, topicName, clonedMessage, cancellationToken);
        await DeleteMessageAsync(connection, topicName, subscriptionName, azureMessage.MessageId, true, cancellationToken);
    }

    public async Task<long> PurgeMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        bool isDlq,
        CancellationToken cancellationToken)
    {
        string path = $"{topicName}/Subscriptions/{subscriptionName}";
        path = isDlq ? $"{path}/$DeadLetterQueue" : path;

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
                _options.CurrentValue.Query.TopicMessageFetchCount,
                operationTimeout,
                cancellationToken);
            if (messages.Count == 0) break;

            purgedCount += messages.Count;
        }

        return purgedCount;
    }

    public async Task<long> TransferDlqMessagesAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        var path = $"{topicName}/Subscriptions/{subscriptionName}/$DeadLetterQueue";

        long transferredCount = 0;
        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            path,
            new ServiceBusReceiverOptions
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            });
        ServiceBusSender sender = client.CreateSender(topicName);

        TimeSpan operationTimeout = TimeSpan.FromSeconds(5);
        while (true)
        {
            IReadOnlyList<ServiceBusReceivedMessage> messages = await receiver.ReceiveMessagesAsync(
                _options.CurrentValue.Query.TopicMessageFetchCount,
                operationTimeout,
                cancellationToken);
            if (messages.Count == 0) break;

            IEnumerable<AzureMessage> messagesToSend = messages.Select(m => new AzureMessage(m));
            await sender.SendMessagesAsync(messagesToSend, cancellationToken);

            transferredCount += messages.Count;
        }

        return transferredCount;
    }

    public async Task CreateTopicAsync(
        ServiceBusConnection connection,
        string topicName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        await client.CreateTopicAsync(topicName, cancellationToken);
    }

    public async Task DeleteTopicAsync(
        ServiceBusConnection connection,
        string topicName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        await client.DeleteTopicAsync(topicName, cancellationToken);
    }

    public async Task CreateSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        await client.CreateSubscriptionAsync(topicName, subscriptionName, cancellationToken);
    }

    public async Task DeleteSubscriptionAsync(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        ServiceBusAdministrationClient client = GetManagementClient(connection);
        await client.DeleteSubscriptionAsync(topicName, subscriptionName, cancellationToken);
    }

    private async Task<ServiceBusReceivedMessage> PeekDlqMessageBySequenceNumber(
        ServiceBusConnection connection,
        string topicName,
        string subscriptionName,
        long sequenceNumber,
        CancellationToken cancellationToken)
    {
        var path = $"{topicName}/Subscriptions/{subscriptionName}/$DeadLetterQueue";

        await using ServiceBusClient client = GetServiceBusClient(connection);
        ServiceBusReceiver receiver = client.CreateReceiver(
            path,
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
