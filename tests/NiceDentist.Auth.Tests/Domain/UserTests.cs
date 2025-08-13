using System;
using FluentAssertions;
using Xunit;
using NiceDentist.Auth.Domain;

namespace NiceDentist.Auth.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_ShouldHaveDefaultValues_WhenCreated()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().Be(0);
        user.Username.Should().Be(string.Empty);
        user.PasswordHash.Should().Be(string.Empty);
        user.Email.Should().Be(string.Empty);
        user.Role.Should().Be("Client");
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_ShouldAllowSettingProperties()
    {
        // Arrange
        var user = new User();
        var testDate = DateTime.UtcNow.AddDays(-1);

        // Act
        user.Id = 123;
        user.Username = "testuser";
        user.PasswordHash = "hashedpassword";
        user.Email = "test@example.com";
        user.Role = "Admin";
        user.CreatedAt = testDate;
        user.IsActive = false;

        // Assert
        user.Id.Should().Be(123);
        user.Username.Should().Be("testuser");
        user.PasswordHash.Should().Be("hashedpassword");
        user.Email.Should().Be("test@example.com");
        user.Role.Should().Be("Admin");
        user.CreatedAt.Should().Be(testDate);
        user.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Manager")]
    [InlineData("Dentist")]
    [InlineData("Customer")]
    public void User_ShouldAcceptValidRoles(string role)
    {
        // Arrange
        var user = new User();

        // Act
        user.Role = role;

        // Assert
        user.Role.Should().Be(role);
    }
}
