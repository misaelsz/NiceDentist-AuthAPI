using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiceDentist.Auth.Application.Auth;

namespace NiceDentist.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    public record RegisterRequest(string Username, string Email, string Password, string Role = "Admin");
    public record LoginRequest(string Username, string Password);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var (ok, message) = await _auth.RegisterAsync(req.Username, req.Email, req.Password, req.Role, ct);
        if (!ok) return BadRequest(new { message });
        return Ok(new { message });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var (ok, token, message) = await _auth.LoginAsync(req.Username, req.Password, ct);
        if (!ok) return Unauthorized(new { message });
        return Ok(new { token });
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public() => Ok(new { message = "Public endpoint" });

    [HttpGet("protected")]
    [Authorize]
    public IActionResult Protected() => Ok(new { message = "Protected endpoint", user = User.Identity?.Name });

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult Admin() => Ok(new { message = "Admin endpoint" });
}
