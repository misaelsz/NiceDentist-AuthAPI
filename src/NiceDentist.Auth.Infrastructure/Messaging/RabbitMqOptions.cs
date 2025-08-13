namespace NiceDentist.Auth.Infrastructure.Messaging;

/// <summary>
/// Configuration settings for RabbitMQ connection
/// </summary>
public class RabbitMqOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "RabbitMQ";
    
    /// <summary>
    /// RabbitMQ server hostname
    /// </summary>
    public string HostName { get; set; } = "localhost";
    
    /// <summary>
    /// RabbitMQ server port
    /// </summary>
    public int Port { get; set; } = 5672;
    
    /// <summary>
    /// Username for authentication
    /// </summary>
    public string UserName { get; set; } = "guest";
    
    /// <summary>
    /// Password for authentication
    /// </summary>
    public string Password { get; set; } = "guest";
    
    /// <summary>
    /// Virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";
    
    /// <summary>
    /// Exchange name for events
    /// </summary>
    public string ExchangeName { get; set; } = "nicedentist.events";
    
    /// <summary>
    /// Queue name for incoming Manager events
    /// </summary>
    public string ManagerEventsQueue { get; set; } = "auth.manager.events";
    
    /// <summary>
    /// Queue name for outgoing User events
    /// </summary>
    public string UserEventsQueue { get; set; } = "manager.user.created";
}
