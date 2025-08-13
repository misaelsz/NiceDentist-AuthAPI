using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiceDentist.Auth.Application.Auth;

namespace NiceDentist.Auth.Api.Controllers;

/// <summary>
/// Controller responsible for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    
    /// <summary>
    /// Initializes a new instance of the AuthController
    /// </summary>
    /// <param name="auth">The authentication service</param>
    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    /// <summary>
    /// Request model for user registration
    /// </summary>
    /// <param name="Username">The username for the new user</param>
    /// <param name="Email">The email for the new user</param>
    /// <param name="Password">The password for the new user</param>
    /// <param name="Role">The role for the new user (default: Admin)</param>
    public record RegisterRequest(string Username, string Email, string Password, string Role = "Admin");
    
    /// <summary>
    /// Request model for user login
    /// </summary>
    /// <param name="Username">The username for login</param>
    /// <param name="Password">The password for login</param>
    public record LoginRequest(string Username, string Password);

    /// <summary>
    /// Registers a new user in the system
    /// </summary>
    /// <param name="req">The registration request containing user details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success or error message</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var (ok, message) = await _auth.RegisterAsync(req.Username, req.Email, req.Password, req.Role, ct);
        if (!ok) return BadRequest(new { message });
        return Ok(new { message });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="req">The login request containing credentials</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>JWT token on success or error message</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var (ok, token, message) = await _auth.LoginAsync(req.Username, req.Password, ct);
        if (!ok) return Unauthorized(new { message });
        return Ok(new { token });
    }

    /// <summary>
    /// Public endpoint that doesn't require authentication
    /// </summary>
    /// <returns>Public message</returns>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public() => Ok(new { message = "Public endpoint" });

    /// <summary>
    /// Protected endpoint that requires authentication
    /// </summary>
    /// <returns>Protected message with user info</returns>
    [HttpGet("protected")]
    [Authorize]
    public IActionResult Protected() => Ok(new { message = "Protected endpoint", user = User.Identity?.Name });

    /// <summary>
    /// Admin-only endpoint that requires Admin role
    /// </summary>
    /// <returns>Admin message</returns>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult Admin() => Ok(new { message = "Admin endpoint" });
}
