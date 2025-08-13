using System.Linq;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Infrastructure;
using Testcontainers.MsSql;

namespace NiceDentist.Auth.IntegrationTests.Infrastructure;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithPassword("Test_Password123!")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing IUserRepository registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add the test database repository
            var connectionString = _dbContainer.GetConnectionString();
            services.AddSingleton<IUserRepository>(_ => new SqlUserRepository(connectionString));
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        // Create the database schema
        var connectionString = _dbContainer.GetConnectionString();
        var repo = new SqlUserRepository(connectionString);
        
        // Initialize the database schema (you might want to create a schema setup method)
        await SetupDatabaseSchema(connectionString);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    private static async Task SetupDatabaseSchema(string connectionString)
    {
        // Add your schema creation logic here
        // For now, simplified version
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync();
        
        var createTableSql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
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
        
        using var command = new Microsoft.Data.SqlClient.SqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
