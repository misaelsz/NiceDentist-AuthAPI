using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Application.Events;
using RabbitMQ.Client;

namespace NiceDentist.Auth.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of event publisher
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of RabbitMqEventPublisher
    /// </summary>
    /// <param name="options">RabbitMQ configuration options</param>
    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
        
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange
        _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true);

        // Declare queues
        _channel.QueueDeclare(_options.UserEventsQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.UserEventsQueue, _options.ExchangeName, "user.*");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Publishes an event to the default exchange
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        var routingKey = GetRoutingKey(@event.EventType);
        return PublishToExchangeAsync(@event, _options.ExchangeName, routingKey, cancellationToken);
    }

    /// <summary>
    /// Publishes an event to a specific queue
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <param name="queueName">Target queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public Task PublishToQueueAsync<T>(T @event, string queueName, CancellationToken cancellationToken = default) where T : BaseEvent
    {
        // Ensure queue exists
        _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        
        var message = JsonSerializer.Serialize(@event, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = @event.EventId.ToString();
        properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)@event.OccurredAt).ToUnixTimeSeconds());
        properties.Type = @event.EventType;

        _channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: properties, body: body);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Publishes an event to a specific exchange with routing key
    /// </summary>
    private Task PublishToExchangeAsync<T>(T @event, string exchangeName, string routingKey, CancellationToken cancellationToken) where T : BaseEvent
    {
        var message = JsonSerializer.Serialize(@event, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = @event.EventId.ToString();
        properties.Timestamp = new AmqpTimestamp(((DateTimeOffset)@event.OccurredAt).ToUnixTimeSeconds());
        properties.Type = @event.EventType;

        _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: properties, body: body);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets routing key for event type
    /// </summary>
    private static string GetRoutingKey(string eventType)
    {
        return eventType.ToLowerInvariant() switch
        {
            "usercreated" => "user.created",
            "useractivated" => "user.activated",
            _ => $"user.{eventType.ToLowerInvariant()}"
        };
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
