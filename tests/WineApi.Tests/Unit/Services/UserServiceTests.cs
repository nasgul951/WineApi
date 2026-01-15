using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Model.User;
using WineApi.Service;
using WineApi.Tests.Fixtures;
using FluentAssertions.Collections;
using WineApi.Exceptions;
using WineApi.Helpers;

namespace WineApi.Tests.Unit.Services;

[TestFixture]
public class UserServiceTests
{
    private WineContext _context;
    private UserService _userService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<WineContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new WineContext(options);
        _userService = new UserService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose(); 
    }

    [Test]
    public void GetByFilter_WithNoFilters_ReturnsAllUsers()
    {
        ArrangeTestUsers();

        // Act
        var req = new UserRequest();
        var result = _userService.GetByFilter(req);

        // Assert
        result.Should().HaveCount(3);
    }

    [Test]
    public void GetByFilter_WithFilter_ReturnsFilteredUsers()
    {
        ArrangeTestUsers();

        // Act
        var req = new UserRequest()
        {
            Username = "test"
        };
        var result = _userService.GetByFilter(req);

        // Assert
        result.Should().HaveCount(1);
    }

    [Test]
    public async Task GetById_WithValidId_ReturnsUser()
    {
        ArrangeTestUsers();

        //Act
        var result = await _userService.GetById(2);

        //Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
    }

    [Test]
    public async Task GetById_WithInvalidId_ThrowsNotFoundException()
    {
        ArrangeTestUsers();

        var act = async () => await _userService.GetById(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task AddUser_WithUniqueUsername_AddsNewUser()
    {
        ArrangeTestUsers();

        var req = new AddUpdateUser()
        {
            Username = "newuser",
            Password = "password",
            IsAdmin = true
        };
        var result = await _userService.AddUser(req);

        // Assert
        var dbUser = _context.Users.Single(u => u.Username == "newuser");
        dbUser.Salt.Should().NotBeEmpty();
        dbUser.Id.Should().Be(result!.Id);
        dbUser.IsAdmin.Should().BeTrue();
    }

    [Test]
    public async Task AddUser_WithExistingUsername_ShouldThrowConflictException()
    {
        ArrangeTestUsers();

        var req = new AddUpdateUser()
        {
            Username = "bob",
            Password = "password",
            IsAdmin = false
        };
        var act = async () => await _userService.AddUser(req);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Test]
    public async Task UpdateUser_WithValidId_AndOnlyUsername_ShouldOnlyUpdatUsername()
    {
        ArrangeTestUsers();


        var req = new AddUpdateUser()
        {
            Username = "tom"
        };
        var result = await _userService.UpdateUser(2, req);

        // Assert
        result.Should().NotBeNull();
        var dbUser = _context.Users.Single(u => u.Id == 2);
        dbUser.Username.Should().Be("tom");
        dbUser.Password.Should().Be("password2");
        dbUser.IsAdmin.Should().BeTrue();
    }

    [Test]
    public async Task UpdateUser_WithValidId_AndOnlyPassword_ShouldOnlyUpdatePassword()
    {
        ArrangeTestUsers();

        var req = new AddUpdateUser()
        {
            Password = "newPassword"
        };
        var result = await _userService.UpdateUser(2, req);

        // Assert
        var dbUser = _context.Users.Single(u => u.Id == 2);
        dbUser.Username.Should().Be("bob");
        dbUser.IsAdmin.Should().BeTrue();
        dbUser.Password.Should().NotBe("password2");
    }

    [Test]
    public async Task UpateUser_WithInvalidId_ShouldThrowNotFoundException()
    {
        ArrangeTestUsers();

        var req = new AddUpdateUser()
        {
            Password = "somepassword"
        };
        var act = async () => await _userService.UpdateUser(999, req);

        // Asert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private void ArrangeTestUsers()
    {
        //Arrange
        var user1 = TestDataBuilder.CreateTestUser(id: 1, username: "testuser1");
        var user2 = TestDataBuilder.CreateTestUser(id: 2, username: "bob", password: "password2", isAdmin: true);
        var user3 = TestDataBuilder.CreateTestUser(id: 3, username: "alice");

        _context.Users.AddRange(user1, user2, user3);
        _context.SaveChanges();
    }
}
