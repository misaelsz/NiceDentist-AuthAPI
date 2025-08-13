using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;
using NiceDentist.Auth.Domain;
using NiceDentist.Auth.Infrastructure;
using Testcontainers.MsSql;

namespace NiceDentist.Auth.IntegrationTests.Infrastructure;

public class SqlUserRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithPassword("Test_Password123!")
        .WithCleanUp(true)
        .Build();

    private SqlUserRepository _repository = null!;
    private string _connectionString = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _connectionString = _dbContainer.GetConnectionString();
        _repository = new SqlUserRepository(_connectionString);
        
        await SetupDatabaseSchema();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateUser_WhenValidUser()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var id = await _repository.CreateAsync(user);

        // Assert
        id.Should().BeGreaterThan(0);
        
        var createdUser = await _repository.GetByIdAsync(id);
        createdUser.Should().NotBeNull();
        createdUser!.Username.Should().Be("testuser");
        createdUser.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Username = "findme",
            Email = "findme@example.com",
            PasswordHash = "hashedpassword",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.CreateAsync(user);

        // Act
        var foundUser = await _repository.GetByUsernameAsync("findme");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Username.Should().Be("findme");
        foundUser.Email.Should().Be("findme@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Username = "emailtest",
            Email = "emailtest@example.com",
            PasswordHash = "hashedpassword",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _repository.CreateAsync(user);

        // Act
        var foundUser = await _repository.GetByEmailAsync("emailtest@example.com");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Email.Should().Be("emailtest@example.com");
        foundUser.Username.Should().Be("emailtest");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenValidUser()
    {
        // Arrange
        var user = new User
        {
            Username = "updateme",
            Email = "updateme@example.com",
            PasswordHash = "hashedpassword",
            Role = "Customer",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var id = await _repository.CreateAsync(user);
        var createdUser = await _repository.GetByIdAsync(id);
        
        // Modify user
        createdUser!.Email = "updated@example.com";
        createdUser.Role = "Admin";

        // Act
        var result = await _repository.UpdateAsync(createdUser);

        // Assert
        result.Should().BeTrue();
        
        var updatedUser = await _repository.GetByIdAsync(id);
        updatedUser!.Email.Should().Be("updated@example.com");
        updatedUser.Role.Should().Be("Admin");
    }

    private async Task SetupDatabaseSchema()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var createTableSql = @"
            CREATE TABLE Users (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                Username NVARCHAR(100) NOT NULL UNIQUE,
                PasswordHash NVARCHAR(256) NOT NULL,
                Email NVARCHAR(256) NOT NULL UNIQUE,
                Role NVARCHAR(50) NOT NULL,
                CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                IsActive BIT NOT NULL DEFAULT 1
            )";
        
        using var command = new SqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
