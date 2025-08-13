# NiceDentist Auth API - Test Documentation

## Test Structure

This project follows Clean Architecture principles with tests organized by architectural layers:

```
tests/
├── NiceDentist.Auth.Tests/                 # Unit Tests
│   ├── Domain/                             # Domain Layer Tests
│   │   └── UserTests.cs                    # User entity tests
│   ├── Application/                        # Application Layer Tests
│   │   ├── AuthServiceTests.cs             # Authentication business logic tests
│   │   └── JwtTokenServiceTests.cs         # JWT token generation tests
│   └── Infrastructure/                     # Infrastructure Layer Tests (future)
│       └── [Repository tests, etc.]
└── NiceDentist.Auth.IntegrationTests/      # Integration Tests
    ├── Controllers/                        # API Controller tests
    │   └── AuthControllerIntegrationTests.cs
    └── Infrastructure/                     # Database integration tests
        ├── SqlUserRepositoryIntegrationTests.cs
        └── IntegrationTestWebAppFactory.cs
```

## Test Categories

### Unit Tests (`NiceDentist.Auth.Tests`)
- **Fast execution** - No external dependencies
- **Domain Tests**: Entity validation, business rules
- **Application Tests**: Service logic, use cases
- **Infrastructure Tests**: Data access patterns (mocked)

### Integration Tests (`NiceDentist.Auth.IntegrationTests`)
- **End-to-end testing** - Real dependencies
- **Controller Tests**: HTTP API endpoints
- **Repository Tests**: Database operations with real SQL Server containers
- **Web Application Tests**: Full application stack testing

## Test Coverage

| Layer | Test Count | Coverage |
|-------|------------|----------|
| Domain | 6 tests | User entity validation |
| Application | 17 tests | Auth & JWT services |
| Integration | 8 tests | API endpoints & database |
| **Total** | **31 tests** | **Comprehensive coverage** |

## Running Tests

### All Tests
```powershell
.\test-runner.ps1
```

### Unit Tests Only
```powershell
.\test-runner.ps1 -TestType unit
```

### Integration Tests Only
```powershell
.\test-runner.ps1 -TestType integration
```

### With Coverage Report
```powershell
.\test-runner.ps1 -TestType coverage -OpenReport
```

## Test Patterns

### Unit Test Pattern
```csharp
[Fact]
public async Task Method_Should_ReturnExpected_When_ValidCondition()
{
    // Arrange
    var service = new ServiceUnderTest();
    
    // Act
    var result = await service.MethodAsync();
    
    // Assert
    result.Should().BeExpected();
}
```

### Integration Test Pattern
```csharp
[Fact]
public async Task Endpoint_Should_ReturnExpected_When_ValidRequest()
{
    // Arrange
    var request = new HttpRequestMessage();
    
    // Act
    var response = await _client.SendAsync(request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Test Dependencies

- **xUnit** - Test framework
- **FluentAssertions** - Assertion library
- **Moq** - Mocking framework
- **Testcontainers** - Docker containers for integration tests
- **Microsoft.AspNetCore.Mvc.Testing** - ASP.NET Core testing

## Notes

- Integration tests require Docker for SQL Server containers
- All tests follow AAA pattern (Arrange, Act, Assert)
- Test names follow convention: `Method_Should_ReturnWhat_When_Condition`
- Edge cases and error scenarios are comprehensively covered
