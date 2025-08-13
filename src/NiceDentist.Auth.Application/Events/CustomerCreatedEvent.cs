namespace NiceDentist.Auth.Application.Events;

/// <summary>
/// Event triggered when a customer is created in the Manager API
/// This event is received by the Auth API to create a corresponding user account
/// </summary>
public class CustomerCreatedEvent : BaseEvent
{
    /// <summary>
    /// Type identifier for the event
    /// </summary>
    public override string EventType => "CustomerCreated";
    
    /// <summary>
    /// Customer data from Manager API
    /// </summary>
    public CustomerCreatedData Data { get; init; } = null!;
}

/// <summary>
/// Data payload for CustomerCreated event
/// </summary>
public class CustomerCreatedData
{
    /// <summary>
    /// Customer ID from Manager database
    /// </summary>
    public int CustomerId { get; init; }
    
    /// <summary>
    /// Customer's full name
    /// </summary>
    public string Name { get; init; } = null!;
    
    /// <summary>
    /// Customer's email address (will be used as username)
    /// </summary>
    public string Email { get; init; } = null!;
    
    /// <summary>
    /// Customer's phone number
    /// </summary>
    public string Phone { get; init; } = null!;
}
