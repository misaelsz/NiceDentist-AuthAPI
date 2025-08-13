using FluentAssertions;
using Xunit;
using NiceDentist.Auth.Application.Auth;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Tests.Application;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateAccessToken_Should_Return_Jwt()
    {
        var svc = new JwtTokenService("test-key-1234567890-long-enough-key", "issuer");
        var token = svc.GenerateAccessToken(new User { Id = 1, Username = "ana", Email = "ana@example.com", Role = "Admin" });
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Length.Should().Be(3);
    }
}
