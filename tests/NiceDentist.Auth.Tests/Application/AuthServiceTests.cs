using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using NiceDentist.Auth.Application.Auth;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Tests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly Mock<IJwtTokenService> _mockJwt;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _mockJwt = new Mock<IJwtTokenService>();
        _authService = new AuthService(_mockRepo.Object, _mockJwt.Object);
    }

    [Theory]
    [InlineData("", "email@test.com", "password", "Username, email and password are required.")]
    [InlineData("user", "", "password", "Username, email and password are required.")]
    [InlineData("user", "email@test.com", "", "Username, email and password are required.")]
    [InlineData(null, "email@test.com", "password", "Username, email and password are required.")]
    public async Task RegisterAsync_ShouldReturnError_WhenInvalidInput(string username, string email, string password, string expectedMessage)
    {
        // Act
        var (ok, message) = await _authService.RegisterAsync(username, email, password);

        // Assert
        ok.Should().BeFalse();
        message.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnError_WhenUsernameExists()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByUsernameAsync("existinguser", default))
               .ReturnsAsync(new User { Username = "existinguser" });

        // Act
        var (ok, message) = await _authService.RegisterAsync("existinguser", "email@test.com", "password");

        // Assert
        ok.Should().BeFalse();
        message.Should().Be("Username already exists.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnError_WhenEmailExists()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByUsernameAsync("newuser", default))
               .ReturnsAsync((User)null!);
        _mockRepo.Setup(r => r.GetByEmailAsync("existing@test.com", default))
               .ReturnsAsync(new User { Email = "existing@test.com" });

        // Act
        var (ok, message) = await _authService.RegisterAsync("newuser", "existing@test.com", "password");

        // Assert
        ok.Should().BeFalse();
        message.Should().Be("Email already exists.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnError_WhenCreateFails()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByUsernameAsync("newuser", default))
               .ReturnsAsync((User)null!);
        _mockRepo.Setup(r => r.GetByEmailAsync("new@test.com", default))
               .ReturnsAsync((User)null!);
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>(), default))
               .ReturnsAsync(0); // Simulate failure

        // Act
        var (ok, message) = await _authService.RegisterAsync("newuser", "new@test.com", "password");

        // Assert
        ok.Should().BeFalse();
        message.Should().Be("Failed to create user.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenValidData()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByUsernameAsync("newuser", default))
               .ReturnsAsync((User)null!);
        _mockRepo.Setup(r => r.GetByEmailAsync("new@test.com", default))
               .ReturnsAsync((User)null!);
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>(), default))
               .ReturnsAsync(1);

        // Act
        var (ok, message) = await _authService.RegisterAsync("newuser", "new@test.com", "password", "Customer");

        // Assert
        ok.Should().BeTrue();
        message.Should().Be("User created.");
        
        // Verify the user creation was called with correct parameters
        _mockRepo.Verify(r => r.CreateAsync(It.Is<User>(u => 
            u.Username == "newuser" && 
            u.Email == "new@test.com" && 
            u.Role == "Customer" &&
            u.IsActive), default), Times.Once);
    }

    [Theory]
    [InlineData("", "password", "Username and password are required.")]
    [InlineData("user", "", "Username and password are required.")]
    [InlineData(null, "password", "Username and password are required.")]
    public async Task LoginAsync_ShouldReturnError_WhenInvalidInput(string username, string password, string expectedMessage)
    {
        // Act
        var (ok, token, message) = await _authService.LoginAsync(username, password);

        // Assert
        ok.Should().BeFalse();
        token.Should().BeEmpty();
        message.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByUsernameAsync("nonexistent", default))
               .ReturnsAsync((User)null!);

        // Act
        var (ok, token, message) = await _authService.LoginAsync("nonexistent", "password");

        // Assert
        ok.Should().BeFalse();
        token.Should().BeEmpty();
        message.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenUserInactive()
    {
        // Arrange
        var inactiveUser = new User { Username = "inactive", IsActive = false };
        _mockRepo.Setup(r => r.GetByUsernameAsync("inactive", default))
               .ReturnsAsync(inactiveUser);

        // Act
        var (ok, token, message) = await _authService.LoginAsync("inactive", "password");

        // Assert
        ok.Should().BeFalse();
        token.Should().BeEmpty();
        message.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnError_WhenPasswordInvalid()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User 
        { 
            Username = "testuser", 
            PasswordHash = hashedPassword, 
            IsActive = true 
        };
        _mockRepo.Setup(r => r.GetByUsernameAsync("testuser", default))
               .ReturnsAsync(user);

        // Act
        var (ok, token, message) = await _authService.LoginAsync("testuser", "wrongpassword");

        // Assert
        ok.Should().BeFalse();
        token.Should().BeEmpty();
        message.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = new User 
        { 
            Id = 1,
            Username = "testuser", 
            PasswordHash = hashedPassword, 
            Email = "test@example.com",
            Role = "Customer",
            IsActive = true 
        };
        _mockRepo.Setup(r => r.GetByUsernameAsync("testuser", default))
               .ReturnsAsync(user);
        _mockJwt.Setup(j => j.GenerateAccessToken(user))
              .Returns("valid-jwt-token");

        // Act
        var (ok, token, message) = await _authService.LoginAsync("testuser", "correctpassword");

        // Assert
        ok.Should().BeTrue();
        token.Should().Be("valid-jwt-token");
        message.Should().Be("Login successful.");
        
        // Verify JWT service was called with correct user
        _mockJwt.Verify(j => j.GenerateAccessToken(user), Times.Once);
    }

    // Additional basic tests for compatibility
    [Fact]
    public async Task Register_Should_Create_User_When_Valid()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), default)).ReturnsAsync((User)null!);
        _mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User)null!);
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>(), default)).ReturnsAsync(1);

        // Act
        var (ok, msg) = await _authService.RegisterAsync("ana", "ana@example.com", "Pass123!");

        // Assert
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Correct()
    {
        // Arrange
        var hash = BCrypt.Net.BCrypt.HashPassword("Pass123!");
        var user = new User { Id = 1, Username = "ana", Email = "ana@example.com", PasswordHash = hash, Role = "Admin", IsActive = true };
        _mockRepo.Setup(r => r.GetByUsernameAsync("ana", default)).ReturnsAsync(user);
        _mockJwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("token");

        // Act
        var (ok, token, _) = await _authService.LoginAsync("ana", "Pass123!");

        // Assert
        ok.Should().BeTrue();
        token.Should().NotBeNullOrEmpty();
    }
}
