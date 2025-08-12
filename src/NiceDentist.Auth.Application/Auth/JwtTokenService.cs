using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Application.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly string _key;
    private readonly string _issuer;

    public JwtTokenService(string key, string issuer)
    {
        _key = key;
        _issuer = issuer;
    }

    public string GenerateAccessToken(User user)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return handler.WriteToken(token);
    }
}
