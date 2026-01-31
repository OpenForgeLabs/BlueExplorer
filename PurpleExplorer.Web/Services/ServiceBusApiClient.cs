using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PurpleExplorer.Web.Models;

namespace PurpleExplorer.Web.Services;

public class ServiceBusApiClient
{
    private readonly HttpClient _httpClient;

    public ServiceBusApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<IReadOnlyList<ConnectionInfo>> GetConnectionsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<ConnectionInfo>>("api/connections", cancellationToken);
    }

    public Task<ConnectionUpsertRequest> GetConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}";
        return GetAsync<ConnectionUpsertRequest>(uri, cancellationToken);
    }

    public Task<bool> GetConnectionHealthAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/health";
        return GetAsync<bool>(uri, cancellationToken);
    }

    public Task CreateConnectionAsync(ConnectionUpsertRequest request, CancellationToken cancellationToken = default)
    {
        return PostAsync("api/connections", request, cancellationToken);
    }

    public Task UpdateConnectionAsync(string connectionName, ConnectionUpsertRequest request, CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}";
        return PutAsync(uri, request, cancellationToken);
    }

    public Task DeleteConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}";
        return DeleteAsync(uri, cancellationToken);
    }

    public Task<NamespaceInfo> GetNamespaceAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/namespace";
        return GetAsync<NamespaceInfo>(uri, cancellationToken);
    }

    public Task<PagedResult<TopicInfo>> GetTopicsAsync(
        string connectionName,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics?pageSize={pageSize}&continuationToken={EscapeToken(continuationToken)}";
        return GetAsync<PagedResult<TopicInfo>>(uri, cancellationToken);
    }

    public Task<PagedResult<SubscriptionInfo>> GetSubscriptionsAsync(
        string connectionName,
        string topicName,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        string uri =
            $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions?pageSize={pageSize}&continuationToken={EscapeToken(continuationToken)}";
        return GetAsync<PagedResult<SubscriptionInfo>>(uri, cancellationToken);
    }

    public Task<PagedResult<QueueInfo>> GetQueuesAsync(
        string connectionName,
        int pageSize,
        string? continuationToken,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues?pageSize={pageSize}&continuationToken={EscapeToken(continuationToken)}";
        return GetAsync<PagedResult<QueueInfo>>(uri, cancellationToken);
    }

    public Task<PagedResult<MessageDto>> GetQueueMessagesAsync(
        string connectionName,
        string queueName,
        bool isDlq,
        int pageSize,
        long? continuationToken,
        CancellationToken cancellationToken = default)
    {
        string route = isDlq ? "dlq" : "messages";
        string uri =
            $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}/{route}?pageSize={pageSize}&continuationToken={continuationToken}";
        return GetAsync<PagedResult<MessageDto>>(uri, cancellationToken);
    }

    public Task<PagedResult<MessageDto>> GetSubscriptionMessagesAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        bool isDlq,
        int pageSize,
        long? continuationToken,
        CancellationToken cancellationToken = default)
    {
        string route = isDlq ? "dlq" : "messages";
        string uri =
            $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions/{Escape(subscriptionName)}/{route}?pageSize={pageSize}&continuationToken={continuationToken}";
        return GetAsync<PagedResult<MessageDto>>(uri, cancellationToken);
    }

    public Task SendQueueMessageAsync(
        string connectionName,
        string queueName,
        string content,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}/messages";
        return PostAsync(uri, new SendMessageRequest { Content = content }, cancellationToken);
    }

    public Task SendTopicMessageAsync(
        string connectionName,
        string topicName,
        string content,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/messages";
        return PostAsync(uri, new SendMessageRequest { Content = content }, cancellationToken);
    }

    public Task DeleteQueueMessageAsync(
        string connectionName,
        string queueName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}/messages/delete";
        return PostAsync(uri, new MessageActionRequest { MessageId = messageId, IsDlq = isDlq }, cancellationToken);
    }

    public Task DeleteSubscriptionMessageAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        string messageId,
        bool isDlq,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions/{Escape(subscriptionName)}/messages/delete";
        return PostAsync(uri, new MessageActionRequest { MessageId = messageId, IsDlq = isDlq }, cancellationToken);
    }

    public Task DeadletterQueueMessageAsync(
        string connectionName,
        string queueName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}/messages/deadletter";
        return PostAsync(uri, new MessageActionRequest { MessageId = messageId }, cancellationToken);
    }

    public Task DeadletterSubscriptionMessageAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions/{Escape(subscriptionName)}/messages/deadletter";
        return PostAsync(uri, new MessageActionRequest { MessageId = messageId }, cancellationToken);
    }

    public Task ResubmitQueueMessageAsync(
        string connectionName,
        string queueName,
        long sequenceNumber,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}/messages/resubmit";
        return PostAsync(uri, new MessageActionRequest { SequenceNumber = sequenceNumber }, cancellationToken);
    }

    public Task ResubmitSubscriptionMessageAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        long sequenceNumber,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions/{Escape(subscriptionName)}/messages/resubmit";
        return PostAsync(uri, new MessageActionRequest { SequenceNumber = sequenceNumber }, cancellationToken);
    }

    public Task<long> PurgeQueueAsync(
        string connectionName,
        string queueName,
        bool isDlq,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}/purge";
        return PostForResultAsync<long>(uri, new PurgeRequest { IsDlq = isDlq }, cancellationToken);
    }

    public Task<long> PurgeSubscriptionAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        bool isDlq,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions/{Escape(subscriptionName)}/purge";
        return PostForResultAsync<long>(uri, new PurgeRequest { IsDlq = isDlq }, cancellationToken);
    }

    public Task<long> TransferQueueDlqAsync(
        string connectionName,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}/transfer-dlq";
        return PostForResultAsync<long>(uri, new object(), cancellationToken);
    }

    public Task<long> TransferSubscriptionDlqAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions/{Escape(subscriptionName)}/transfer-dlq";
        return PostForResultAsync<long>(uri, new object(), cancellationToken);
    }

    public Task CreateQueueAsync(
        string connectionName,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues";
        return PostAsync(uri, new CreateQueueRequest { Name = queueName }, cancellationToken);
    }

    public Task DeleteQueueAsync(
        string connectionName,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/queues/{Escape(queueName)}";
        return DeleteAsync(uri, cancellationToken);
    }

    public Task CreateTopicAsync(
        string connectionName,
        string topicName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics";
        return PostAsync(uri, new CreateTopicRequest { Name = topicName }, cancellationToken);
    }

    public Task DeleteTopicAsync(
        string connectionName,
        string topicName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}";
        return DeleteAsync(uri, cancellationToken);
    }

    public Task CreateSubscriptionAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions";
        return PostAsync(uri, new CreateSubscriptionRequest { Name = subscriptionName }, cancellationToken);
    }

    public Task DeleteSubscriptionAsync(
        string connectionName,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken = default)
    {
        string uri = $"api/connections/{Escape(connectionName)}/topics/{Escape(topicName)}/subscriptions/{Escape(subscriptionName)}";
        return DeleteAsync(uri, cancellationToken);
    }

    private async Task<T> GetAsync<T>(string uri, CancellationToken cancellationToken)
    {
        T? result = await _httpClient.GetFromJsonAsync<T>(uri, cancellationToken);
        if (result == null)
            throw new InvalidOperationException($"Empty response from {uri}.");

        return result;
    }

    private async Task PostAsync<T>(string uri, T payload, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task PutAsync<T>(string uri, T payload, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(uri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task DeleteAsync(string uri, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<TResult> PostForResultAsync<TResult>(string uri, object payload, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uri, payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        TResult? result = await response.Content.ReadFromJsonAsync<TResult>(cancellationToken: cancellationToken);
        if (result == null)
            throw new InvalidOperationException($"Empty response from {uri}.");

        return result;
    }

    private static string Escape(string value) => Uri.EscapeDataString(value);
    private static string EscapeToken(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.EscapeDataString(value);

    private class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }

    private class MessageActionRequest
    {
        public string MessageId { get; set; } = string.Empty;
        public long SequenceNumber { get; set; }
        public bool IsDlq { get; set; }
    }

    private class PurgeRequest
    {
        public bool IsDlq { get; set; }
    }

    private class CreateQueueRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    private class CreateTopicRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    private class CreateSubscriptionRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
