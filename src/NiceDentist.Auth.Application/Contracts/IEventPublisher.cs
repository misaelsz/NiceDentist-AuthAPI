using NiceDentist.Auth.Application.Events;

namespace NiceDentist.Auth.Application.Contracts;

/// <summary>
/// Interface for publishing events to message broker
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the specified queue/exchange
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : BaseEvent;
    
    /// <summary>
    /// Publishes an event to a specific queue
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <param name="queueName">Target queue name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishToQueueAsync<T>(T @event, string queueName, CancellationToken cancellationToken = default) where T : BaseEvent;
}
