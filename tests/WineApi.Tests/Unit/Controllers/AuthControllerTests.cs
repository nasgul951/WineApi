using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WineApi.Controllers;
using WineApi.Service;

namespace WineApi.Tests.Unit.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _mockAuthService;
    private Mock<ILogger<AuthController>> _mockLogger;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Test]
    public void Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var request = new AuthRequest { UserName = "testuser", Password = "password123" };
        var expectedResponse = new AuthResponse { Token = "test-token", Expires = DateTime.UtcNow.AddDays(7) };

        _mockAuthService
            .Setup(x => x.Authenticate(request.UserName, request.Password))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedResponse);
    }

    [Test]
    public void Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new AuthRequest { UserName = "testuser", Password = "wrongpassword" };

        _mockAuthService
            .Setup(x => x.Authenticate(request.UserName, request.Password))
            .ReturnsAsync((AuthResponse?)null);

        // Act
        var result = _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().Be("Invalid username or password.");
    }

    [Test]
    public void Login_WhenExceptionOccurs_Returns500()
    {
        // Arrange
        var request = new AuthRequest { UserName = "testuser", Password = "password123" };

        _mockAuthService
            .Setup(x => x.Authenticate(request.UserName, request.Password))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = _controller.Login(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
        objectResult.Value!.ToString().Should().Contain("Internal server error");
    }
}
