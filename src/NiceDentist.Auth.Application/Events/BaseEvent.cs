namespace NiceDentist.Auth.Application.Events;

/// <summary>
/// Base class for all events in the system
/// </summary>
public abstract class BaseEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// Type of the event
    /// </summary>
    public abstract string EventType { get; }
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Source service that generated the event
    /// </summary>
    public string Source { get; init; } = "NiceDentist.Auth.Api";
    
    /// <summary>
    /// Version of the event schema
    /// </summary>
    public string Version { get; init; } = "1.0";
}
