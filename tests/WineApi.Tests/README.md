# WineApi.Tests

This project contains unit and integration tests for the WineApi application using NUnit.

## Project Structure

- **Unit/** - Unit tests that mock dependencies
  - **Controllers/** - Tests for API controllers
  - **Services/** - Tests for business logic services
  - **Extensions/** - Tests for extension methods

- **Integration/** - Integration tests with database
  - **Database/** - Tests for data access and EF Core context
  - **Authentication/** - Tests for authentication flow

- **Fixtures/** - Test helpers and utilities
  - `TestDataBuilder.cs` - Builder pattern for creating test data
  - `WineContextFixture.cs` - In-memory database setup

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~AuthControllerTests.Login_WithValidCredentials_ReturnsOkWithToken"

# Run tests in a specific namespace
dotnet test --filter "FullyQualifiedName~WineApi.Tests.Unit.Controllers"
```

## Writing Tests

### Unit Test Example

```csharp
[TestFixture]
public class MyServiceTests
{
    private Mock<IDependency> _mockDependency;
    private MyService _service;

    [SetUp]
    public void Setup()
    {
        _mockDependency = new Mock<IDependency>();
        _service = new MyService(_mockDependency.Object);
    }

    [Test]
    public void MyMethod_WhenCalled_ReturnsExpectedResult()
    {
        // Arrange
        _mockDependency.Setup(x => x.DoSomething()).Returns("expected");

        // Act
        var result = _service.MyMethod();

        // Assert
        result.Should().Be("expected");
    }
}
```

### Integration Test Example

```csharp
[TestFixture]
public class DatabaseTests
{
    private WineContext _context;

    [SetUp]
    public void Setup()
    {
        _context = WineContextFixture.CreateInMemoryContext();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task CanSaveAndRetrieveWine()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine();

        // Act
        _context.Wines.Add(wine);
        await _context.SaveChangesAsync();

        // Assert
        var saved = await _context.Wines.FindAsync(wine.Id);
        saved.Should().NotBeNull();
        saved.Varietal.Should().Be(wine.Varietal);
    }
}
```

## Dependencies

- **NUnit** - Test framework
- **NUnit3TestAdapter** - VS test runner
- **Microsoft.NET.Test.Sdk** - .NET test SDK
- **Moq** - Mocking framework
- **FluentAssertions** - Fluent assertion library
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing** - ASP.NET Core integration testing
