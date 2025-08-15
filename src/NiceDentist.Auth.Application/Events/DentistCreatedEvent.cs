namespace NiceDentist.Auth.Application.Events;

/// <summary>
/// Event triggered when a dentist is created in the Manager API
/// This event is received by the Auth API to create a corresponding user account
/// </summary>
public class DentistCreatedEvent : BaseEvent
{
    /// <summary>
    /// Type identifier for the event
    /// </summary>
    public override string EventType => "dentistcreated";
    
    /// <summary>
    /// Dentist data from Manager API
    /// </summary>
    public DentistCreatedData Data { get; init; } = null!;
}

/// <summary>
/// Data payload for DentistCreated event
/// </summary>
public class DentistCreatedData
{
    /// <summary>
    /// Dentist ID from Manager database
    /// </summary>
    public int DentistId { get; init; }
    
    /// <summary>
    /// Dentist's full name
    /// </summary>
    public string Name { get; init; } = null!;
    
    /// <summary>
    /// Dentist's email address (will be used as username)
    /// </summary>
    public string Email { get; init; } = null!;
    
    /// <summary>
    /// Dentist's phone number
    /// </summary>
    public string Phone { get; init; } = null!;
    
    /// <summary>
    /// Professional license number
    /// </summary>
    public string LicenseNumber { get; init; } = null!;
    
    /// <summary>
    /// Dentist's specialization
    /// </summary>
    public string? Specialization { get; init; }
}
