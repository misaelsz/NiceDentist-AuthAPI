using NiceDentist.Auth.Application.Auth;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Api;

/// <summary>
/// Static class responsible for seeding initial data
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seeds the database with initial admin user if it doesn't exist
    /// </summary>
    /// <param name="app">The web application instance</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task SeedAsync(this WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting data seeding...");
            
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            
            // Try to create admin user - if it exists, this will fail gracefully
            try
            {
                logger.LogInformation("Creating admin user...");
                var hash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                await repo.CreateAsync(new User
                {
                    Username = "admin",
                    Email = "admin@nicedentist.com",
                    PasswordHash = hash,
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                logger.LogInformation("Admin user created successfully");
            }
            catch (Exception createEx)
            {
                logger.LogWarning(createEx, "Failed to create admin user (might already exist)");
                
                // Check if user exists by username or email
                var userByUsername = await repo.GetByUsernameAsync("admin");
                var userByEmail = await repo.GetByEmailAsync("admin@nicedentist.com");
                
                if (userByUsername != null)
                {
                    logger.LogInformation("Admin user exists by username: {Username}", userByUsername.Username);
                }
                if (userByEmail != null)
                {
                    logger.LogInformation("Admin user exists by email: {Email}", userByEmail.Email);
                }
                
                if (userByUsername == null && userByEmail == null)
                {
                    logger.LogError("Admin user creation failed and user doesn't exist!");
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error during data seeding");
            throw;
        }
    }
}
