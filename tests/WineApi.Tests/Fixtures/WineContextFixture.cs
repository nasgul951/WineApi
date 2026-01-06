using Microsoft.EntityFrameworkCore;
using WineApi.Data;

namespace WineApi.Tests.Fixtures;

/// <summary>
/// Fixture for creating in-memory database contexts for testing
/// </summary>
public class WineContextFixture
{
    public static WineContext CreateInMemoryContext(string databaseName = "TestDatabase")
    {
        var options = new DbContextOptionsBuilder<WineContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        var context = new WineContext(options);
        return context;
    }

    public static WineContext CreateContextWithData()
    {
        var context = CreateInMemoryContext(Guid.NewGuid().ToString());

        // Add test data
        var user = TestDataBuilder.CreateTestUser();
        var wine = TestDataBuilder.CreateTestWine();
        var storage = TestDataBuilder.CreateTestStorage();
        var bottle = TestDataBuilder.CreateTestBottle();

        context.Users.Add(user);
        context.Wines.Add(wine);
        context.Storages.Add(storage);
        context.Bottles.Add(bottle);
        context.SaveChanges();

        return context;
    }
}
