using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Application.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NiceDentist.Auth.Infrastructure.Messaging;

/// <summary>
/// Background service that consumes events from RabbitMQ
/// </summary>
public class RabbitMqEventConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventConsumer> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of RabbitMqEventConsumer
    /// </summary>
    /// <param name="options">RabbitMQ configuration options</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="logger">Logger instance</param>
    public RabbitMqEventConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqEventConsumer> logger)
    {
        _options = options.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;

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

        // Declare and bind queue for Manager events
        _channel.QueueDeclare(_options.ManagerEventsQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_options.ManagerEventsQueue, _options.ExchangeName, "manager.*");
        
        // Also bind specific event types
        _channel.QueueBind(_options.ManagerEventsQueue, _options.ExchangeName, "customer.created");
        _channel.QueueBind(_options.ManagerEventsQueue, _options.ExchangeName, "dentist.created");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Executes the background service
    /// </summary>
    /// <param name="stoppingToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (sender, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var eventType = eventArgs.BasicProperties.Type;

                _logger.LogInformation("Received event of type {EventType}", eventType);

                await ProcessEventAsync(eventType, message, stoppingToken);

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event");
                _channel.BasicNack(eventArgs.DeliveryTag, false, true); // Requeue on error
            }
        };

        _channel.BasicConsume(_options.ManagerEventsQueue, autoAck: false, consumer);

        _logger.LogInformation("Started consuming events from queue {QueueName}", _options.ManagerEventsQueue);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Processes an incoming event based on its type
    /// </summary>
    /// <param name="eventType">Type of the event</param>
    /// <param name="message">Event message JSON</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    private async Task ProcessEventAsync(string eventType, string message, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            switch (eventType?.ToLowerInvariant())
            {
                case "customercreated":
                    var customerEvent = JsonSerializer.Deserialize<CustomerCreatedEvent>(message, _jsonOptions);
                    if (customerEvent != null)
                    {
                        var customerHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<CustomerCreatedEvent>>();
                        await customerHandler.HandleAsync(customerEvent, cancellationToken);
                    }
                    break;

                case "dentistcreated":
                    var dentistEvent = JsonSerializer.Deserialize<DentistCreatedEvent>(message, _jsonOptions);
                    if (dentistEvent != null)
                    {
                        var dentistHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<DentistCreatedEvent>>();
                        await dentistHandler.HandleAsync(dentistEvent, cancellationToken);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event of type {EventType}", eventType);
            throw;
        }
    }

    /// <summary>
    /// Disposes resources when the service stops
    /// </summary>
    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
