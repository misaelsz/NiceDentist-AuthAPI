namespace NiceDentist.Auth.Application.Events;

/// <summary>
/// Event triggered when a user is created in the Auth API
/// This event is sent to the Manager API to link the UserId with Customer/Dentist
/// </summary>
public class UserCreatedEvent : BaseEvent
{
    /// <summary>
    /// Type identifier for the event
    /// </summary>
    public override string EventType => "UserCreated";
    
    /// <summary>
    /// User data from Auth API
    /// </summary>
    public UserCreatedData Data { get; init; } = null!;
}

/// <summary>
/// Data payload for UserCreated event
/// </summary>
public class UserCreatedData
{
    /// <summary>
    /// User ID from Auth database
    /// </summary>
    public int UserId { get; init; }
    
    /// <summary>
    /// User's email address (used to match with Customer/Dentist)
    /// </summary>
    public string Email { get; init; } = null!;
    
    /// <summary>
    /// User's role (Customer or Dentist)
    /// </summary>
    public string Role { get; init; } = null!;
    
    /// <summary>
    /// Type of entity in Manager API (Customer or Dentist)
    /// </summary>
    public string EntityType { get; init; } = null!;
    
    /// <summary>
    /// ID of the entity in Manager API that should be linked
    /// </summary>
    public int EntityId { get; init; }
}
