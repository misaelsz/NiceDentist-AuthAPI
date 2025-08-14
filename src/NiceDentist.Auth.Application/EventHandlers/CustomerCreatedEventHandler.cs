using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Application.Events;
using NiceDentist.Auth.Domain;
using System.Diagnostics;
using System.Web;

namespace NiceDentist.Auth.Application.EventHandlers;

/// <summary>
/// Handles CustomerCreated events from Manager API
/// Creates a new User account for the customer
/// </summary>
public class CustomerCreatedEventHandler : IEventHandler<CustomerCreatedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of the CustomerCreatedEventHandler
    /// </summary>
    /// <param name="userRepository">User repository</param>
    /// <param name="eventPublisher">Event publisher</param>
    public CustomerCreatedEventHandler(IUserRepository userRepository, IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the CustomerCreated event by creating a user account
    /// </summary>
    /// <param name="event">The CustomerCreated event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task HandleAsync(CustomerCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        const string customerRole = "Customer";
        
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(@event.Data.Email);
        if (existingUser != null)
        {
            // User already exists, just publish UserCreated event
            await PublishUserCreatedEvent(existingUser.Id, @event.Data.Email, customerRole, customerRole, @event.Data.CustomerId);
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
            Role = customerRole
        };

        var userId = await _userRepository.CreateAsync(user);

        // Publish UserCreated event to Manager API
        await PublishUserCreatedEvent(userId, @event.Data.Email, customerRole, customerRole, @event.Data.CustomerId);

        // Send welcome email by opening HTML page (simulates email sending)
        await SendWelcomeEmailAsync(@event.Data.Name, @event.Data.Email, defaultPassword);
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

    /// <summary>
    /// Sends welcome email by opening HTML template in browser (simulates email sending)
    /// </summary>
    /// <param name="customerName">Customer's name</param>
    /// <param name="customerEmail">Customer's email</param>
    /// <param name="temporaryPassword">Temporary password</param>
    /// <returns>Task representing the async operation</returns>
    private static async Task SendWelcomeEmailAsync(string customerName, string customerEmail, string temporaryPassword)
    {
        try
        {
            // Build the URL with parameters for the HTML email template
            var encodedName = HttpUtility.UrlEncode(customerName);
            var encodedEmail = HttpUtility.UrlEncode(customerEmail);
            var encodedPassword = HttpUtility.UrlEncode(temporaryPassword);
            
            var emailUrl = $"http://localhost:3000/email-templates/customer-welcome.html" +
                          $"?name={encodedName}" +
                          $"&email={encodedEmail}" +
                          $"&password={encodedPassword}";

            // Open the email template in the default browser (simulates sending email)
            var psi = new ProcessStartInfo
            {
                FileName = emailUrl,
                UseShellExecute = true
            };
            
            Process.Start(psi);
            
            await Task.CompletedTask;
        }
        catch (Exception)
        {
            // Log the error in a real scenario, but don't throw to avoid disrupting the flow
            await Task.CompletedTask;
        }
    }
}
