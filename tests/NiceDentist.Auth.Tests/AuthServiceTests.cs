using FluentAssertions;
using Moq;
using NiceDentist.Auth.Application.Auth;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_Should_Create_User_When_Valid()
    {
        var repo = new Mock<IUserRepository>();
        var jwt = new Mock<IJwtTokenService>();
        repo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>(), default)).ReturnsAsync(1);
        var svc = new AuthService(repo.Object, jwt.Object);

        var (ok, msg) = await svc.RegisterAsync("ana", "ana@example.com", "Pass123!");

        ok.Should().BeTrue();
    }

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Correct()
    {
        var repo = new Mock<IUserRepository>();
        var jwt = new Mock<IJwtTokenService>();
        var hash = BCrypt.Net.BCrypt.HashPassword("Pass123!");
        repo.Setup(r => r.GetByUsernameAsync("ana", default)).ReturnsAsync(new User { Id=1, Username="ana", Email="ana@example.com", PasswordHash=hash, Role="Admin", IsActive=true});
        jwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        var svc = new AuthService(repo.Object, jwt.Object);

        var (ok, token, _) = await svc.LoginAsync("ana", "Pass123!");

        ok.Should().BeTrue();
        token.Should().NotBeNullOrEmpty();
    }
}
