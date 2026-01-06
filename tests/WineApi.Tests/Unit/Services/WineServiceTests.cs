using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Model.Wine;
using WineApi.Service;
using WineApi.Tests.Fixtures;

namespace WineApi.Tests.Unit.Services;

/// <summary>
/// Unit tests for WineService.
/// Each test gets a fresh in-memory database to ensure test isolation.
/// </summary>
[TestFixture]
public class WineServiceTests
{
    private WineContext _context;
    private WineService _wineService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<WineContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new WineContext(options);
        _wineService = new WineService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetWines Tests

    [Test]
    public void GetWines_WithShowAllTrue_ReturnsAllWines()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, varietal: "Cabernet");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, varietal: "Merlot");

        _context.Wines.AddRange(wine1, wine2);
        _context.SaveChanges();

        var request = new WineRequest { ShowAll = true };

        // Act
        var result = _wineService.GetWines(request).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Test]
    public void GetWines_WithShowAllFalse_ReturnsOnlyWinesWithAvailableBottles()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, varietal: "Cabernet");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, varietal: "Merlot");
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        bottle1.Consumed = 0; // Available

        _context.Wines.AddRange(wine1, wine2);
        _context.Storages.Add(storage);
        _context.Bottles.Add(bottle1);
        _context.SaveChanges();

        var request = new WineRequest { ShowAll = false };

        // Act
        var result = _wineService.GetWines(request).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
    }

    [Test]
    public void GetWines_WithVarietalFilter_ReturnsMatchingWines()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, varietal: "Cabernet");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, varietal: "Merlot");

        _context.Wines.AddRange(wine1, wine2);
        _context.SaveChanges();

        var request = new WineRequest { Varietal = "Cabernet", ShowAll = true };

        // Act
        var result = _wineService.GetWines(request).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Varietal.Should().Be("Cabernet");
    }

    [Test]
    public void GetWines_WithVineyardFilter_ReturnsMatchingWines()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, vineyard: "Napa Valley");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, vineyard: "Sonoma");

        _context.Wines.AddRange(wine1, wine2);
        _context.SaveChanges();

        var request = new WineRequest { Vineyard = "Napa Valley", ShowAll = true };

        // Act
        var result = _wineService.GetWines(request).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Vineyard.Should().Be("Napa Valley");
    }

    [Test]
    public void GetWines_WithIdFilter_ReturnsSingleWine()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1);
        var wine2 = TestDataBuilder.CreateTestWine(id: 2);

        _context.Wines.AddRange(wine1, wine2);
        _context.SaveChanges();

        var request = new WineRequest { Id = 1, ShowAll = true };

        // Act
        var result = _wineService.GetWines(request).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
    }

    #endregion

    #region GetVarietals Tests

    [Test]
    public void GetVarietals_ReturnsUniqueVarietalsWithCounts()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, varietal: "Cabernet");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, varietal: "Cabernet");
        var wine3 = TestDataBuilder.CreateTestWine(id: 3, varietal: "Merlot");
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        var bottle2 = TestDataBuilder.CreateTestBottle(id: 2, wineId: 2, storageId: 1);
        var bottle3 = TestDataBuilder.CreateTestBottle(id: 3, wineId: 3, storageId: 1);

        bottle1.Consumed = 0;
        bottle2.Consumed = 0;
        bottle3.Consumed = 0;

        _context.Wines.AddRange(wine1, wine2, wine3);
        _context.Storages.Add(storage);
        _context.Bottles.AddRange(bottle1, bottle2, bottle3);
        _context.SaveChanges();

        // Act
        var result = _wineService.GetVarietals().ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Name == "Cabernet" && v.Count == 2);
        result.Should().Contain(v => v.Name == "Merlot" && v.Count == 1);
    }

    [Test]
    public void GetVarietals_OnlyIncludesWinesWithAvailableBottles()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, varietal: "Cabernet");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, varietal: "Merlot");
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        var bottle2 = TestDataBuilder.CreateTestBottle(id: 2, wineId: 2, storageId: 1);

        bottle1.Consumed = 0; // Available
        bottle2.Consumed = 1; // Consumed

        _context.Wines.AddRange(wine1, wine2);
        _context.Storages.Add(storage);
        _context.Bottles.AddRange(bottle1, bottle2);
        _context.SaveChanges();

        // Act
        var result = _wineService.GetVarietals().ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Cabernet");
    }

    [Test]
    public void GetVarietals_ReturnsResultsInAlphabeticalOrder()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, varietal: "Zinfandel");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, varietal: "Cabernet");
        var wine3 = TestDataBuilder.CreateTestWine(id: 3, varietal: "Merlot");
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        var bottle2 = TestDataBuilder.CreateTestBottle(id: 2, wineId: 2, storageId: 1);
        var bottle3 = TestDataBuilder.CreateTestBottle(id: 3, wineId: 3, storageId: 1);

        bottle1.Consumed = 0;
        bottle2.Consumed = 0;
        bottle3.Consumed = 0;

        _context.Wines.AddRange(wine1, wine2, wine3);
        _context.Storages.Add(storage);
        _context.Bottles.AddRange(bottle1, bottle2, bottle3);
        _context.SaveChanges();

        // Act
        var result = _wineService.GetVarietals().ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Cabernet");
        result[1].Name.Should().Be("Merlot");
        result[2].Name.Should().Be("Zinfandel");
    }

    #endregion

    #region GetVineyards Tests

    [Test]
    public void GetVineyards_WithoutFilter_ReturnsAllVineyards()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1, vineyard: "Napa Valley");
        var wine2 = TestDataBuilder.CreateTestWine(id: 2, vineyard: "Sonoma");
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        var bottle2 = TestDataBuilder.CreateTestBottle(id: 2, wineId: 2, storageId: 1);

        bottle1.Consumed = 0;
        bottle2.Consumed = 0;

        _context.Wines.AddRange(wine1, wine2);
        _context.Storages.Add(storage);
        _context.Bottles.AddRange(bottle1, bottle2);
        _context.SaveChanges();

        // Act
        var result = _wineService.GetVineyards(null).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region AddWine Tests

    [Test]
    public async Task AddWine_CreatesNewWineWithCorrectProperties()
    {
        // Arrange
        var newWine = new Wine
        {
            Varietal = "Pinot Noir",
            Vineyard = "Willamette Valley",
            Label = "Reserve",
            Vintage = 2020,
            Notes = "Excellent vintage"
        };

        // Act
        var result = await _wineService.AddWine(newWine);

        // Assert
        result.Should().NotBeNull();
        result.Varietal.Should().Be("Pinot Noir");
        result.Vineyard.Should().Be("Willamette Valley");
        result.Label.Should().Be("Reserve");
        result.Vintage.Should().Be(2020);
        result.Notes.Should().Be("Excellent vintage");

        // Verify it was added to the database
        var dbWine = await _context.Wines.FirstOrDefaultAsync();
        dbWine.Should().NotBeNull();
        dbWine!.Varietal.Should().Be("Pinot Noir");
    }

    [Test]
    public async Task AddWine_SetsCreatedDate()
    {
        // Arrange
        var newWine = new Wine
        {
            Varietal = "Chardonnay",
            Vineyard = "Russian River",
            Label = "Estate",
            Vintage = 2021
        };

        // Act
        var result = await _wineService.AddWine(newWine);

        // Assert
        var dbWine = await _context.Wines.FirstOrDefaultAsync();
        dbWine.Should().NotBeNull();
        dbWine!.CreatedDate.Should().NotBeNull();
        dbWine.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region UpdateWine Tests

    [Test]
    public async Task UpdateWine_UpdatesSpecifiedFields()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(
            id: 1,
            varietal: "Old Varietal",
            vineyard: "Old Vineyard",
            label: "Old Label"
        );

        _context.Wines.Add(wine);
        await _context.SaveChangesAsync();

        var updateRequest = new WinePatchRequest
        {
            Varietal = "New Varietal",
            Label = "New Label"
            // Vineyard intentionally not updated
        };

        // Act
        var result = await _wineService.UpdateWine(1, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Varietal.Should().Be("New Varietal");
        result.Label.Should().Be("New Label");
        result.Vineyard.Should().Be("Old Vineyard"); // Should remain unchanged
    }

    [Test]
    public async Task UpdateWine_WithAllFields_UpdatesAllProperties()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(id: 1);
        _context.Wines.Add(wine);
        await _context.SaveChangesAsync();

        var updateRequest = new WinePatchRequest
        {
            Varietal = "Updated Varietal",
            Vineyard = "Updated Vineyard",
            Label = "Updated Label",
            Vintage = 2022,
            Notes = "Updated notes"
        };

        // Act
        var result = await _wineService.UpdateWine(1, updateRequest);

        // Assert
        result.Varietal.Should().Be("Updated Varietal");
        result.Vineyard.Should().Be("Updated Vineyard");
        result.Label.Should().Be("Updated Label");
        result.Vintage.Should().Be(2022);
        result.Notes.Should().Be("Updated notes");
    }

    #endregion

    #region AddBottle Tests

    [Test]
    public async Task AddBottle_CreatesNewBottleWithCorrectProperties()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(id: 1);
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        _context.Wines.Add(wine);
        _context.Storages.Add(storage);
        await _context.SaveChangesAsync();

        var newBottle = new PutBottle
        {
            WineId = 1,
            StorageId = 1,
            BinX = 5,
            BinY = 3,
            Depth = 2
        };

        // Act
        var result = await _wineService.AddBottle(newBottle);

        // Assert
        result.Should().NotBeNull();
        result.WineId.Should().Be(1);
        result.StorageId.Should().Be(1);
        result.BinX.Should().Be(5);
        result.BinY.Should().Be(3);
        result.Depth.Should().Be(2);

        // Verify it was added to the database
        var dbBottle = await _context.Bottles.FirstOrDefaultAsync();
        dbBottle.Should().NotBeNull();
        dbBottle!.Wineid.Should().Be(1);
    }

    [Test]
    public async Task AddBottle_SetsCreatedDate()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(id: 1);
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        _context.Wines.Add(wine);
        _context.Storages.Add(storage);
        await _context.SaveChangesAsync();

        var newBottle = new PutBottle
        {
            WineId = 1,
            StorageId = 1,
            BinX = 1,
            BinY = 1,
            Depth = 1
        };

        // Act
        await _wineService.AddBottle(newBottle);

        // Assert
        var dbBottle = await _context.Bottles.FirstOrDefaultAsync();
        dbBottle!.CreatedDate.Should().NotBeNull();
        dbBottle.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region UpdateBottle Tests

    [Test]
    public async Task UpdateBottle_UpdatesSpecifiedFields()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(id: 1);
        var storage = TestDataBuilder.CreateTestStorage(id: 1);
        var bottle = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1, binX: 1, binY: 1);

        _context.Wines.Add(wine);
        _context.Storages.Add(storage);
        _context.Bottles.Add(bottle);
        await _context.SaveChangesAsync();

        var updateRequest = new PatchBottle
        {
            BinX = 5,
            BinY = 3
            // Other fields not updated
        };

        // Act
        var result = await _wineService.UpdateBottle(1, updateRequest);

        // Assert
        result.BinX.Should().Be(5);
        result.BinY.Should().Be(3);
        result.WineId.Should().Be(1); // Unchanged
    }

    [Test]
    public async Task UpdateBottle_CanMarkAsConsumed()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(id: 1);
        var storage = TestDataBuilder.CreateTestStorage(id: 1);
        var bottle = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);

        _context.Wines.Add(wine);
        _context.Storages.Add(storage);
        _context.Bottles.Add(bottle);
        await _context.SaveChangesAsync();

        var updateRequest = new PatchBottle { Consumed = true };

        // Act
        await _wineService.UpdateBottle(1, updateRequest);

        // Assert
        var dbBottle = await _context.Bottles.FindAsync(1);
        dbBottle!.Consumed.Should().Be(1);
    }

    #endregion

    #region GetBottles Tests

    [Test]
    public void GetBottles_WithNoFilters_ReturnsAllAvailableBottles()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(id: 1);
        var storage = TestDataBuilder.CreateTestStorage(id: 1);
        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        var bottle2 = TestDataBuilder.CreateTestBottle(id: 2, wineId: 1, storageId: 1);

        bottle1.Consumed = 0;
        bottle2.Consumed = 0;

        _context.Wines.Add(wine);
        _context.Storages.Add(storage);
        _context.Bottles.AddRange(bottle1, bottle2);
        _context.SaveChanges();

        // Act
        var result = _wineService.GetBottles(null, null).ToList();

        // Assert
        result.Should().HaveCount(2);
    }

    [Test]
    public void GetBottles_OnlyReturnsNonConsumedBottles()
    {
        // Arrange
        var wine = TestDataBuilder.CreateTestWine(id: 1);
        var storage = TestDataBuilder.CreateTestStorage(id: 1);
        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        var bottle2 = TestDataBuilder.CreateTestBottle(id: 2, wineId: 1, storageId: 1);

        bottle1.Consumed = 0; // Available
        bottle2.Consumed = 1; // Consumed

        _context.Wines.Add(wine);
        _context.Storages.Add(storage);
        _context.Bottles.AddRange(bottle1, bottle2);
        _context.SaveChanges();

        // Act
        var result = _wineService.GetBottles(null, null).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
    }

    [Test]
    public void GetBottles_WithWineIdFilter_ReturnsOnlyMatchingBottles()
    {
        // Arrange
        var wine1 = TestDataBuilder.CreateTestWine(id: 1);
        var wine2 = TestDataBuilder.CreateTestWine(id: 2);
        var storage = TestDataBuilder.CreateTestStorage(id: 1);

        var bottle1 = TestDataBuilder.CreateTestBottle(id: 1, wineId: 1, storageId: 1);
        var bottle2 = TestDataBuilder.CreateTestBottle(id: 2, wineId: 2, storageId: 1);

        bottle1.Consumed = 0;
        bottle2.Consumed = 0;

        _context.Wines.AddRange(wine1, wine2);
        _context.Storages.Add(storage);
        _context.Bottles.AddRange(bottle1, bottle2);
        _context.SaveChanges();

        // Act
        var result = _wineService.GetBottles(wineId: 1, binId: null).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].WineId.Should().Be(1);
    }

    #endregion
}
