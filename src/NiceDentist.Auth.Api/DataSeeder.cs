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
        using var scope = app.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = await repo.GetByUsernameAsync("admin");
        if (user == null)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            await repo.CreateAsync(new User
            {
                Username = "admin",
                Email = "admin@nicedentist.local",
                PasswordHash = hash,
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
        }
    }
}
