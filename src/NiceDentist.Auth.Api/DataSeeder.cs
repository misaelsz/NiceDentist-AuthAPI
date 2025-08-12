using NiceDentist.Auth.Application.Auth;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Api;

public static class DataSeeder
{
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
