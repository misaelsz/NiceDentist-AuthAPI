using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Application.Events;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Application.EventHandlers;

/// <summary>
/// Handles DentistCreated events from Manager API
/// Creates a new User account for the dentist
/// </summary>
public class DentistCreatedEventHandler : IEventHandler<DentistCreatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of the DentistCreatedEventHandler
    /// </summary>
    /// <param name="userRepository">User repository</param>
    /// <param name="eventPublisher">Event publisher</param>
    public DentistCreatedEventHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the DentistCreated event by creating a user account
    /// </summary>
    /// <param name="event">The DentistCreated event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task HandleAsync(DentistCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(@event.Data.Email);
        if (existingUser != null)
        {
            // User already exists, just publish UserCreated event
            await PublishUserCreatedEvent(existingUser.Id, @event.Data.Email, "Dentist", "Dentist", @event.Data.DentistId);
            return;
        }

        // Generate default password (should be changed on first login)
        var defaultPassword = GenerateDefaultPassword();
        
        // Create new user
        var user = new User
        {
            Username = @event.Data.Email, // Use email as username
            Email = @event.Data.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
            Role = "Dentist"
        };

        var userId = await _userRepository.CreateAsync(user);

        // Publish UserCreated event to Manager API
        await PublishUserCreatedEvent(userId, @event.Data.Email, "Dentist", "Dentist", @event.Data.DentistId);

        // TODO: Send welcome email with temporary password
    }

    /// <summary>
    /// Publishes UserCreated event to Manager API
    /// </summary>
    private async Task PublishUserCreatedEvent(int userId, string email, string role, string entityType, int entityId)
    {
        var userCreatedEvent = new UserCreatedEvent
        {
            Data = new UserCreatedData
            {
                UserId = userId,
                Email = email,
                Role = role,
                EntityType = entityType,
                EntityId = entityId
            }
        };

        await _eventPublisher.PublishToQueueAsync(userCreatedEvent, "manager.user.created", CancellationToken.None);
    }

    /// <summary>
    /// Generates a default password for new users
    /// </summary>
    private static string GenerateDefaultPassword()
    {
        // Generate a temporary password (8 characters)
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
