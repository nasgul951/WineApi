using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Exceptions;
using WineApi.Helpers;
using WineApi.Service;
using WineApi.Tests.Fixtures;

namespace WineApi.Tests.Unit.Services;

/// <summary>
/// Unit tests for AuthService.
/// Each test gets a fresh in-memory database to ensure test isolation.
/// </summary>
[TestFixture]
public class AuthServiceTests
{
    private WineContext _context;
    private AuthService _authService;

    /// <summary>
    /// Runs before each test. Creates a new in-memory database with a unique name
    /// to ensure complete isolation between tests.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<WineContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        _context = new WineContext(options);
        _authService = new AuthService(_context);
    }

    /// <summary>
    /// Runs after each test. Ensures the database is deleted and disposed
    /// to free up resources.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task Authenticate_WithValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var salt = "testsalt";
        var password = "password123";
        var hashedPassword = CryptoHelper.HashPassword(password, salt);

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = hashedPassword,
            Salt = salt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.Authenticate("testuser", password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Token.Should().HaveLength(32); // GUID without hyphens
        result.Expires.Should().BeAfter(DateTime.UtcNow);
        result.Expires.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task Authenticate_WithInvalidUsername_ReturnsNull()
    {
        // Arrange - Fresh database, only one user
        var user = TestDataBuilder.CreateTestUser(username: "testuser");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.Authenticate("nonexistentuser", "password123");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Authenticate_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        var salt = "testsalt";
        var password = "correctpassword";
        var hashedPassword = CryptoHelper.HashPassword(password, salt);

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = hashedPassword,
            Salt = salt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.Authenticate("testuser", "wrongpassword");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Authenticate_UpdatesUserKeyAndExpiration()
    {
        // Arrange
        var salt = "testsalt";
        var password = "password123";
        var hashedPassword = CryptoHelper.HashPassword(password, salt);

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = hashedPassword,
            Salt = salt,
            Key = null,
            KeyExpires = null
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.Authenticate("testuser", password);

        // Assert
        var updatedUser = await _context.Users.FindAsync(1);
        updatedUser.Should().NotBeNull();
        updatedUser!.Key.Should().NotBeNullOrEmpty();
        updatedUser.Key.Should().HaveLength(32); // GUID without hyphens
        updatedUser.KeyExpires.Should().NotBeNull();
        updatedUser.KeyExpires.Should().BeAfter(DateTime.UtcNow);
        updatedUser.LastOn.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task GetUserByToken_WithValidToken_ReturnsUser()
    {
        // Arrange
        var token = "validtoken123";
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = "hashedpassword",
            Salt = "salt",
            Key = token,
            KeyExpires = DateTime.UtcNow.AddDays(7)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetUserByToken(token);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Username.Should().Be("testuser");
        result.Key.Should().Be(token);
    }

    [Test]
    public async Task GetUserByToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange
        var token = "expiredtoken123";
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = "hashedpassword",
            Salt = "salt",
            Key = token,
            KeyExpires = DateTime.UtcNow.AddDays(-1) // Expired yesterday
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetUserByToken(token);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetUserByToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = "hashedpassword",
            Salt = "salt",
            Key = "validtoken123",
            KeyExpires = DateTime.UtcNow.AddDays(7)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetUserByToken("wrongtoken");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetUserByToken_WithNullToken_ReturnsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = "hashedpassword",
            Salt = "salt",
            Key = "sometoken",
            KeyExpires = DateTime.UtcNow.AddDays(7)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetUserByToken(null!);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ClearUserToken_WithValidUserId_ClearsTokenAndExpiration()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = "hashedpassword",
            Salt = "salt",
            Key = "sometoken",
            KeyExpires = DateTime.UtcNow.AddDays(7)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        await _authService.ClearUserToken(1);

        // Assert
        var updatedUser = await _context.Users.FindAsync(1);
        updatedUser.Should().NotBeNull();
        updatedUser!.Key.Should().BeNull();
        updatedUser.KeyExpires.Should().BeNull();
    }

    [Test]
    public void ClearUserToken_WithInvalidUserId_ThrowsInvalidRequestException()
    {
        // Arrange - Empty database (fresh from SetUp)
        // No users added, so ID 999 definitely doesn't exist

        // Act
        Func<Task> act = async () => await _authService.ClearUserToken(999);

        // Assert
        act.Should().ThrowAsync<InvalidRequestException>();
    }

    [Test]
    public async Task Authenticate_MultipleTimes_GeneratesDifferentTokens()
    {
        // Arrange
        var salt = "testsalt";
        var password = "password123";
        var hashedPassword = CryptoHelper.HashPassword(password, salt);

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Password = hashedPassword,
            Salt = salt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result1 = await _authService.Authenticate("testuser", password);
        await Task.Delay(10); // Small delay to ensure different GUIDs
        var result2 = await _authService.Authenticate("testuser", password);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.Token.Should().NotBe(result2!.Token);
    }

    [Test]
    public async Task Authenticate_WithMultipleUsersInDatabase_ReturnsCorrectUser()
    {
        // Arrange - Multiple users in database
        var salt1 = "salt1";
        var salt2 = "salt2";
        var password1 = "password1";
        var password2 = "password2";

        var user1 = new User
        {
            Id = 1,
            Username = "user1",
            Password = CryptoHelper.HashPassword(password1, salt1),
            Salt = salt1
        };

        var user2 = new User
        {
            Id = 2,
            Username = "user2",
            Password = CryptoHelper.HashPassword(password2, salt2),
            Salt = salt2
        };

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.Authenticate("user2", password2);

        // Assert
        result.Should().NotBeNull();
        var authenticatedUser = await _context.Users.FindAsync(2);
        authenticatedUser!.Key.Should().Be(result!.Token);
    }
}
