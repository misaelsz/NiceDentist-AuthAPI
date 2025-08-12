using BCrypt.Net;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Application.Auth;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwt;

    public AuthService(IUserRepository users, IJwtTokenService jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<(bool ok, string message)> RegisterAsync(string username, string email, string password, string role = "Admin", CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Username, email and password are required.");

        if (await _users.GetByUsernameAsync(username, ct) != null)
            return (false, "Username already exists.");
        if (await _users.GetByEmailAsync(email, ct) != null)
            return (false, "Email already exists.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var id = await _users.CreateAsync(user, ct);
        return id > 0 ? (true, "User created.") : (false, "Failed to create user.");
    }

    public async Task<(bool ok, string token, string message)> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, string.Empty, "Username and password are required.");

        var user = await _users.GetByUsernameAsync(username, ct);
        if (user == null || !user.IsActive)
            return (false, string.Empty, "Invalid credentials.");

        var valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!valid)
            return (false, string.Empty, "Invalid credentials.");

        var token = _jwt.GenerateAccessToken(user);
        return (true, token, "Login successful.");
    }
}
