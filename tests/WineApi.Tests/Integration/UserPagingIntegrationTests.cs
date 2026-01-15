using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WineApi.Data;
using WineApi.Model.Base;
using WineApi.Model.User;
using WineApi.Tests.Fixtures;

namespace WineApi.Tests.Integration;

[TestFixture]
public class UserPagingIntegrationTests
{
    private WineApiFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WineApiFactory();
        // CreateClient() initializes the test server and services
        _client = _factory.CreateClient();

        // Clear any existing data for test isolation
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WineContext>();
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task SeedUsersAsync(int count)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WineContext>();

        for (int i = 1; i <= count; i++)
        {
            context.Users.Add(TestDataBuilder.CreateTestUser(
                id: i,
                username: $"user{i:D3}",
                isAdmin: i % 3 == 0,
                key: "valid-key",
                keyExpires: DateTime.UtcNow.AddHours(1)
            ));
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedUsersWithDatesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WineContext>();

        var baseDate = DateTime.UtcNow;

        context.Users.AddRange(
            new User
            {
                Id = 1, Username = "alice", Password = "hash", Salt = "salt",
                LastOn = baseDate.AddDays(-1).ToString("O"), IsAdmin = false,
                Key = "valid-key", KeyExpires = DateTime.UtcNow.AddHours(1)
            },
            new User
            {
                Id = 2, Username = "bob", Password = "hash", Salt = "salt",
                LastOn = baseDate.AddDays(-3).ToString("O"), IsAdmin = true,
                Key = "valid-key", KeyExpires = DateTime.UtcNow.AddHours(1)
            },
            new User
            {
                Id = 3, Username = "charlie", Password = "hash", Salt = "salt",
                LastOn = baseDate.AddDays(-2).ToString("O"), IsAdmin = false,
                Key = "valid-key", KeyExpires = DateTime.UtcNow.AddHours(1)
            }
        );

        await context.SaveChangesAsync();
    }

    [Test]
    public async Task Query_WithDefaultPaging_ReturnsFirstPage()
    {
        await SeedUsersAsync(25);

        var response = await _client.GetAsync("/User/query");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(10); // Default page size
    }

    [Test]
    public async Task Query_WithCustomPageSize_ReturnsRequestedSize()
    {
        await SeedUsersAsync(25);

        var response = await _client.GetAsync("/User/query?pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(5);
    }

    [Test]
    public async Task Query_WithPageNumber_ReturnsCorrectPage()
    {
        await SeedUsersAsync(25);

        var response = await _client.GetAsync("/User/query?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(10);

        // Page 1 should have users 11-20 (0-indexed pages)
        var usernames = result.Items.Select(u => u.Username).ToList();
        usernames.Should().Contain("user011");
        usernames.Should().NotContain("user001");
    }

    [Test]
    public async Task Query_LastPage_ReturnsRemainingItems()
    {
        await SeedUsersAsync(25);

        var response = await _client.GetAsync("/user/query?page=2&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(25);
        result.Items.Should().HaveCount(5); // Only 5 remaining on last page
    }

    [Test]
    public async Task Query_BeyondLastPage_ReturnsEmptyItems()
    {
        await SeedUsersAsync(25);

        var response = await _client.GetAsync("/user/query?page=10&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(25);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task Query_SortByUsernameAsc_ReturnsSortedResults()
    {
        await SeedUsersWithDatesAsync();

        var response = await _client.GetAsync("/user/query?sortField=username&sortDirection=asc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        var usernames = result!.Items.Select(u => u.Username).ToList();
        usernames.Should().BeInAscendingOrder();
        usernames.Should().ContainInOrder("alice", "bob", "charlie");
    }

    [Test]
    public async Task Query_SortByUsernameDesc_ReturnsSortedResults()
    {
        await SeedUsersWithDatesAsync();

        var response = await _client.GetAsync("/user/query?sortField=username&sortDirection=desc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        var usernames = result!.Items.Select(u => u.Username).ToList();
        usernames.Should().BeInDescendingOrder();
        usernames.Should().ContainInOrder("charlie", "bob", "alice");
    }

    [Test]
    public async Task Query_SortByIsAdmin_ReturnsSortedResults()
    {
        await SeedUsersWithDatesAsync();

        var response = await _client.GetAsync("/user/query?sortField=isAdmin&sortDirection=desc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        // Bob is the only admin, should be first when sorting desc
        result!.Items.First().Username.Should().Be("bob");
    }

    [Test]
    public async Task Query_WithUsernameFilter_ReturnsFilteredAndPagedResults()
    {
        await SeedUsersAsync(25);

        var response = await _client.GetAsync("/user/query?username=user01&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        // Should match user010-user019 (10 users starting with "user01")
        result!.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(5);
        result.Items.Should().AllSatisfy(u => u.Username.Should().StartWith("user01"));
    }

    [Test]
    public async Task Query_WithNoResults_ReturnsEmptyPagedResponse()
    {
        await SeedUsersAsync(5);

        var response = await _client.GetAsync("/user/query?username=nonexistent");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserDto>>();

        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Test]
    public async Task Query_TotalCountRemainsConsistentAcrossPages()
    {
        await SeedUsersAsync(25);

        var page0 = await _client.GetFromJsonAsync<PagedResponse<UserDto>>("/user/query?page=0&pageSize=10");
        var page1 = await _client.GetFromJsonAsync<PagedResponse<UserDto>>("/user/query?page=1&pageSize=10");
        var page2 = await _client.GetFromJsonAsync<PagedResponse<UserDto>>("/user/query?page=2&pageSize=10");

        page0!.TotalCount.Should().Be(25);
        page1!.TotalCount.Should().Be(25);
        page2!.TotalCount.Should().Be(25);

        // Verify we get all unique users across pages
        var allUsers = page0.Items.Concat(page1.Items).Concat(page2.Items).ToList();
        allUsers.Should().HaveCount(25);
        allUsers.Select(u => u.Id).Distinct().Should().HaveCount(25);
    }
}
