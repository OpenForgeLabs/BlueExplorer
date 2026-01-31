using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PurpleExplorer.Api.Contracts;
using PurpleExplorer.Api.Services;
using PurpleExplorer.Core.Configuration;
using PurpleExplorer.Core.Models;
using PurpleExplorer.Core.Services;

namespace PurpleExplorer.Api.Controllers;

[ApiController]
[Route("api/connections")]
public class ServiceBusController : ControllerBase
{
    private readonly IServiceBusConnectionProvider _connectionProvider;
    private readonly IConnectionStore _connectionStore;
    private readonly IQueueService _queueService;
    private readonly ITopicService _topicService;
    private readonly IOptionsMonitor<ServiceBusOptions> _options;

    public ServiceBusController(
        IServiceBusConnectionProvider connectionProvider,
        IConnectionStore connectionStore,
        IQueueService queueService,
        ITopicService topicService,
        IOptionsMonitor<ServiceBusOptions> options)
    {
        _connectionProvider = connectionProvider;
        _connectionStore = connectionStore;
        _queueService = queueService;
        _topicService = topicService;
        _options = options;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConnectionSummary>>> GetConnections(
        CancellationToken cancellationToken)
    {
        List<ConnectionSummary> items =
            _options.CurrentValue.Connections.Select(config => new ConnectionSummary
                {
                    Name = config.Name,
                    UseManagedIdentity = config.UseManagedIdentity,
                    IsEditable = false,
                    Source = "appsettings"
                })
                .ToList();

        IReadOnlyList<ServiceBusConnectionConfig> stored = await _connectionStore.GetAllAsync(cancellationToken);
        items.AddRange(stored.Select(config => new ConnectionSummary
        {
            Name = config.Name,
            UseManagedIdentity = config.UseManagedIdentity,
            IsEditable = true,
            Source = "connections.json"
        }));

        return Ok(items);
    }

    [HttpGet("{connectionName}")]
    public async Task<ActionResult<ConnectionUpsertRequest>> GetConnection(
        string connectionName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnectionConfig? config = await _connectionStore.GetAsync(connectionName, cancellationToken);
        if (config != null)
        {
            return Ok(new ConnectionUpsertRequest
            {
                Name = config.Name,
                UseManagedIdentity = config.UseManagedIdentity,
                ConnectionString = config.ConnectionString,
                KeyVault = config.KeyVault
            });
        }

        ServiceBusConnectionConfig? fromSettings = _options.CurrentValue.Connections
            .FirstOrDefault(conn => conn.Name.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
        if (fromSettings == null)
            return NotFound();

        return Ok(new ConnectionUpsertRequest
        {
            Name = fromSettings.Name,
            UseManagedIdentity = fromSettings.UseManagedIdentity,
            ConnectionString = null,
            KeyVault = fromSettings.KeyVault
        });
    }

    [HttpGet("{connectionName}/health")]
    public async Task<ActionResult<bool>> GetConnectionHealth(
        string connectionName,
        CancellationToken cancellationToken)
    {
        try
        {
            ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
            _ = await _topicService.GetNamespaceInfoAsync(connection, cancellationToken);
            return Ok(true);
        }
        catch
        {
            return Ok(false);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateConnection(
        [FromBody] ConnectionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        string validation = ValidateConnectionRequest(request);
        if (!string.IsNullOrWhiteSpace(validation))
            return BadRequest(validation);

        bool existsInSettings = _options.CurrentValue.Connections
            .Any(conn => conn.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
        if (existsInSettings)
            return Conflict($"Connection '{request.Name}' already exists in appsettings.");

        var config = new ServiceBusConnectionConfig
        {
            Name = request.Name,
            UseManagedIdentity = request.UseManagedIdentity,
            ConnectionString = request.ConnectionString,
            KeyVault = request.KeyVault
        };

        await _connectionStore.AddAsync(config, cancellationToken);
        return Ok();
    }

    [HttpPut("{connectionName}")]
    public async Task<IActionResult> UpdateConnection(
        string connectionName,
        [FromBody] ConnectionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!connectionName.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Connection name mismatch.");

        string validation = ValidateConnectionRequest(request);
        if (!string.IsNullOrWhiteSpace(validation))
            return BadRequest(validation);

        var config = new ServiceBusConnectionConfig
        {
            Name = request.Name,
            UseManagedIdentity = request.UseManagedIdentity,
            ConnectionString = request.ConnectionString,
            KeyVault = request.KeyVault
        };

        try
        {
            await _connectionStore.UpdateAsync(config, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{connectionName}")]
    public async Task<IActionResult> DeleteConnection(
        string connectionName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnectionConfig? existing = await _connectionStore.GetAsync(connectionName, cancellationToken);
        if (existing == null)
            return NotFound();

        await _connectionStore.DeleteAsync(connectionName, cancellationToken);
        return Ok();
    }

    [HttpGet("{connectionName}/namespace")]
    public async Task<ActionResult<NamespaceInfo>> GetNamespaceInfo(
        string connectionName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        NamespaceInfo info = await _topicService.GetNamespaceInfoAsync(connection, cancellationToken);
        return Ok(info);
    }

    [HttpGet("{connectionName}/topics")]
    public async Task<ActionResult<PagedResult<TopicInfo>>> GetTopics(
        string connectionName,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? continuationToken = null)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        PagedResult<TopicInfo> topics =
            await _topicService.GetTopicsAsync(connection, pageSize, continuationToken, cancellationToken);
        return Ok(topics);
    }

    [HttpGet("{connectionName}/topics/{topicName}/subscriptions")]
    public async Task<ActionResult<PagedResult<SubscriptionInfo>>> GetSubscriptions(
        string connectionName,
        string topicName,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? continuationToken = null)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        PagedResult<SubscriptionInfo> subscriptions =
            await _topicService.GetSubscriptionsAsync(connection, topicName, pageSize, continuationToken, cancellationToken);
        return Ok(subscriptions);
    }

    [HttpGet("{connectionName}/queues")]
    public async Task<ActionResult<PagedResult<QueueInfo>>> GetQueues(
        string connectionName,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? continuationToken = null)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        PagedResult<QueueInfo> queues =
            await _queueService.GetQueuesAsync(connection, pageSize, continuationToken, cancellationToken);
        return Ok(queues);
    }

    [HttpGet("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/messages")]
    public async Task<ActionResult<PagedResult<MessageDto>>> GetSubscriptionMessages(
        string connectionName,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100,
        [FromQuery] long? continuationToken = null)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        PagedResult<MessageDto> messages =
            await _topicService.GetSubscriptionMessagesAsync(
                connection,
                topicName,
                subscriptionName,
                false,
                pageSize,
                continuationToken,
                cancellationToken);
        return Ok(messages);
    }

    [HttpGet("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/dlq")]
    public async Task<ActionResult<PagedResult<MessageDto>>> GetSubscriptionDlqMessages(
        string connectionName,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100,
        [FromQuery] long? continuationToken = null)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        PagedResult<MessageDto> messages =
            await _topicService.GetSubscriptionMessagesAsync(
                connection,
                topicName,
                subscriptionName,
                true,
                pageSize,
                continuationToken,
                cancellationToken);
        return Ok(messages);
    }

    [HttpGet("{connectionName}/queues/{queueName}/messages")]
    public async Task<ActionResult<PagedResult<MessageDto>>> GetQueueMessages(
        string connectionName,
        string queueName,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100,
        [FromQuery] long? continuationToken = null)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        PagedResult<MessageDto> messages =
            await _queueService.GetMessagesAsync(
                connection,
                queueName,
                false,
                pageSize,
                continuationToken,
                cancellationToken);
        return Ok(messages);
    }

    [HttpGet("{connectionName}/queues/{queueName}/dlq")]
    public async Task<ActionResult<PagedResult<MessageDto>>> GetQueueDlqMessages(
        string connectionName,
        string queueName,
        CancellationToken cancellationToken,
        [FromQuery] int pageSize = 100,
        [FromQuery] long? continuationToken = null)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        PagedResult<MessageDto> messages =
            await _queueService.GetMessagesAsync(
                connection,
                queueName,
                true,
                pageSize,
                continuationToken,
                cancellationToken);
        return Ok(messages);
    }

    [HttpPost("{connectionName}/topics/{topicName}/messages")]
    public async Task<IActionResult> SendTopicMessage(
        string connectionName,
        string topicName,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.SendMessageAsync(connection, topicName, request.Content, cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/queues/{queueName}/messages")]
    public async Task<IActionResult> SendQueueMessage(
        string connectionName,
        string queueName,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _queueService.SendMessageAsync(connection, queueName, request.Content, cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/messages/delete")]
    public async Task<IActionResult> DeleteSubscriptionMessage(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromBody] MessageActionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MessageId))
            return BadRequest("MessageId is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.DeleteMessageAsync(
            connection,
            topicName,
            subscriptionName,
            request.MessageId,
            request.IsDlq,
            cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/queues/{queueName}/messages/delete")]
    public async Task<IActionResult> DeleteQueueMessage(
        string connectionName,
        string queueName,
        [FromBody] MessageActionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MessageId))
            return BadRequest("MessageId is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _queueService.DeleteMessageAsync(
            connection,
            queueName,
            request.MessageId,
            request.IsDlq,
            cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/messages/deadletter")]
    public async Task<IActionResult> DeadletterSubscriptionMessage(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromBody] MessageActionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MessageId))
            return BadRequest("MessageId is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.DeadletterMessageAsync(connection, topicName, subscriptionName, request.MessageId, cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/queues/{queueName}/messages/deadletter")]
    public async Task<IActionResult> DeadletterQueueMessage(
        string connectionName,
        string queueName,
        [FromBody] MessageActionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.MessageId))
            return BadRequest("MessageId is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _queueService.DeadletterMessageAsync(connection, queueName, request.MessageId, cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/messages/resubmit")]
    public async Task<IActionResult> ResubmitSubscriptionMessage(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromBody] MessageActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SequenceNumber <= 0)
            return BadRequest("SequenceNumber is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.ResubmitDlqMessageAsync(
            connection,
            topicName,
            subscriptionName,
            request.SequenceNumber,
            cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/queues/{queueName}/messages/resubmit")]
    public async Task<IActionResult> ResubmitQueueMessage(
        string connectionName,
        string queueName,
        [FromBody] MessageActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SequenceNumber <= 0)
            return BadRequest("SequenceNumber is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _queueService.ResubmitDlqMessageAsync(connection, queueName, request.SequenceNumber, cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/purge")]
    public async Task<ActionResult<long>> PurgeSubscriptionMessages(
        string connectionName,
        string topicName,
        string subscriptionName,
        [FromBody] PurgeRequest request,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        long purged = await _topicService.PurgeMessagesAsync(
            connection,
            topicName,
            subscriptionName,
            request.IsDlq,
            cancellationToken);
        return Ok(purged);
    }

    [HttpPost("{connectionName}/queues/{queueName}/purge")]
    public async Task<ActionResult<long>> PurgeQueueMessages(
        string connectionName,
        string queueName,
        [FromBody] PurgeRequest request,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        long purged = await _queueService.PurgeMessagesAsync(connection, queueName, request.IsDlq, cancellationToken);
        return Ok(purged);
    }

    [HttpPost("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}/transfer-dlq")]
    public async Task<ActionResult<long>> TransferSubscriptionDlq(
        string connectionName,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        long transferred = await _topicService.TransferDlqMessagesAsync(
            connection,
            topicName,
            subscriptionName,
            cancellationToken);
        return Ok(transferred);
    }

    [HttpPost("{connectionName}/queues/{queueName}/transfer-dlq")]
    public async Task<ActionResult<long>> TransferQueueDlq(
        string connectionName,
        string queueName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        long transferred = await _queueService.TransferDlqMessagesAsync(connection, queueName, cancellationToken);
        return Ok(transferred);
    }

    [HttpPost("{connectionName}/queues")]
    public async Task<IActionResult> CreateQueue(
        string connectionName,
        [FromBody] CreateQueueRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _queueService.CreateQueueAsync(connection, request.Name, cancellationToken);
        return Ok();
    }

    [HttpDelete("{connectionName}/queues/{queueName}")]
    public async Task<IActionResult> DeleteQueue(
        string connectionName,
        string queueName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _queueService.DeleteQueueAsync(connection, queueName, cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/topics")]
    public async Task<IActionResult> CreateTopic(
        string connectionName,
        [FromBody] CreateTopicRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.CreateTopicAsync(connection, request.Name, cancellationToken);
        return Ok();
    }

    [HttpDelete("{connectionName}/topics/{topicName}")]
    public async Task<IActionResult> DeleteTopic(
        string connectionName,
        string topicName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.DeleteTopicAsync(connection, topicName, cancellationToken);
        return Ok();
    }

    [HttpPost("{connectionName}/topics/{topicName}/subscriptions")]
    public async Task<IActionResult> CreateSubscription(
        string connectionName,
        string topicName,
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.CreateSubscriptionAsync(connection, topicName, request.Name, cancellationToken);
        return Ok();
    }

    [HttpDelete("{connectionName}/topics/{topicName}/subscriptions/{subscriptionName}")]
    public async Task<IActionResult> DeleteSubscription(
        string connectionName,
        string topicName,
        string subscriptionName,
        CancellationToken cancellationToken)
    {
        ServiceBusConnection connection = await ResolveConnection(connectionName, cancellationToken);
        await _topicService.DeleteSubscriptionAsync(connection, topicName, subscriptionName, cancellationToken);
        return Ok();
    }

    private Task<ServiceBusConnection> ResolveConnection(string connectionName, CancellationToken cancellationToken)
    {
        return _connectionProvider.GetConnectionAsync(connectionName, cancellationToken);
    }

    private static string ValidateConnectionRequest(ConnectionUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Name is required.";

        bool hasConn = !string.IsNullOrWhiteSpace(request.ConnectionString);
        bool hasKeyVault = request.KeyVault != null &&
                           !string.IsNullOrWhiteSpace(request.KeyVault.VaultUri) &&
                           !string.IsNullOrWhiteSpace(request.KeyVault.SecretName);
        if (!hasConn && !hasKeyVault)
            return "Either ConnectionString or KeyVault settings are required.";

        return string.Empty;
    }
}
