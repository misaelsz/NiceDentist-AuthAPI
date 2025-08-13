using NiceDentist.Auth.Application.Events;

namespace NiceDentist.Auth.Application.Contracts;

/// <summary>
/// Interface for handling incoming events from message broker
/// </summary>
public interface IEventHandler<in T> where T : BaseEvent
{
    /// <summary>
    /// Handles the incoming event
    /// </summary>
    /// <param name="event">The event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}
