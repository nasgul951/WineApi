using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using WineApi.Controllers;
using WineApi.Model.Attributes.Varietal;
using WineApi.Model.Wine;
using WineApi.Service;

namespace WineApi.Tests.Unit.Controllers;

/// <summary>
/// Unit tests for WineController.
/// Uses mocked dependencies to test controller behavior in isolation.
/// </summary>
[TestFixture]
public class WineControllerTests
{
    private Mock<IWineService> _mockWineService;
    private Mock<ILogger<WineController>> _mockLogger;
    private WineController _controller;

    [SetUp]
    public void Setup()
    {
        _mockWineService = new Mock<IWineService>();
        _mockLogger = new Mock<ILogger<WineController>>();
        _controller = new WineController(_mockLogger.Object, _mockWineService.Object);
    }

    #region Get (Paged Query) Tests

    // Note: Full end-to-end testing of Get() with PagedRequest is challenging in unit tests
    // because PagedRequest.BuildResponseAsync() uses EF.Property() which only works with
    // actual EF Core queries. Comprehensive testing of pagination and sorting should be
    // done in integration tests with a real or in-memory database context.

    [Test]
    public async Task Get_WithFilterObject_PassesFilterToService()
    {
        // Arrange
        var wines = new List<Wine>().AsQueryable().BuildMock();
        WineRequest? capturedRequest = null;

        _mockWineService
            .Setup(x => x.GetWines(It.IsAny<WineRequest>()))
            .Returns(wines)
            .Callback<WineRequest>(req => capturedRequest = req);

        var request = new PagedRequest<WineRequest, Wine>
        {
            Filter = "{\"varietal\":\"Cabernet\",\"showAll\":true}"
        };

        // Act
        await _controller.Get(request);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Varietal.Should().Be("Cabernet");
        capturedRequest.ShowAll.Should().BeTrue();
    }

    #endregion

    #region GetWine Tests

    [Test]
    public async Task GetWine_WithExistingId_ReturnsOkWithWine()
    {
        // Arrange
        var wine = new Wine
        {
            Id = 1,
            Varietal = "Cabernet",
            Vineyard = "Napa Valley",
            Vintage = 2020
        };

        var wines = new List<Wine> { wine }.AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetWines(It.Is<WineRequest>(r => r.Id == 1 && r.ShowAll == true)))
            .Returns(wines);

        // Act
        var result = await _controller.GetWine(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(wine);
    }

    [Test]
    public async Task GetWine_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var wines = new List<Wine>().AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetWines(It.Is<WineRequest>(r => r.Id == 999)))
            .Returns(wines);

        // Act
        var result = await _controller.GetWine(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Put (Add Wine) Tests

    [Test]
    public async Task Put_WithValidWine_ReturnsCreatedWine()
    {
        // Arrange
        var newWine = new Wine
        {
            Varietal = "Pinot Noir",
            Vineyard = "Willamette Valley",
            Label = "Reserve",
            Vintage = 2021
        };

        var createdWine = new Wine
        {
            Id = 1,
            Varietal = "Pinot Noir",
            Vineyard = "Willamette Valley",
            Label = "Reserve",
            Vintage = 2021
        };

        _mockWineService
            .Setup(x => x.AddWine(newWine))
            .ReturnsAsync(createdWine);

        // Act
        var result = await _controller.Put(newWine);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Varietal.Should().Be("Pinot Noir");
        _mockWineService.Verify(x => x.AddWine(newWine), Times.Once);
    }

    #endregion

    #region Patch (Update Wine) Tests

    [Test]
    public async Task Patch_WithValidUpdate_ReturnsUpdatedWine()
    {
        // Arrange
        var patchRequest = new WinePatchRequest
        {
            Varietal = "Updated Varietal",
            Label = "Updated Label"
        };

        var updatedWine = new Wine
        {
            Id = 1,
            Varietal = "Updated Varietal",
            Label = "Updated Label",
            Vineyard = "Original Vineyard"
        };

        _mockWineService
            .Setup(x => x.UpdateWine(1, patchRequest))
            .ReturnsAsync(updatedWine);

        // Act
        var result = await _controller.Patch(1, patchRequest);

        // Assert
        result.Should().NotBeNull();
        result.Varietal.Should().Be("Updated Varietal");
        result.Label.Should().Be("Updated Label");
        _mockWineService.Verify(x => x.UpdateWine(1, patchRequest), Times.Once);
    }

    #endregion

    #region GetVariatals Tests

    [Test]
    public async Task GetVariatals_ReturnsOkWithVarietalList()
    {
        // Arrange
        var varietals = new List<Varietal>
        {
            new Varietal { Name = "Cabernet", Count = 5 },
            new Varietal { Name = "Merlot", Count = 3 }
        }.AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetVarietals())
            .Returns(varietals);

        // Act
        var result = await _controller.GetVariatals();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var varietalList = okResult!.Value as List<Varietal>;
        varietalList.Should().HaveCount(2);
        varietalList![0].Name.Should().Be("Cabernet");
        varietalList[1].Name.Should().Be("Merlot");
    }

    #endregion

    #region GetVineyards Tests

    [Test]
    public async Task GetVineyards_WithoutFilter_ReturnsOkWithAllVineyards()
    {
        // Arrange
        var vineyards = new List<Vineyard>
        {
            new Vineyard { Name = "Napa Valley", Count = 10 },
            new Vineyard { Name = "Sonoma", Count = 7 }
        }.AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetVineyards(null))
            .Returns(vineyards);

        // Act
        var result = await _controller.GetVineyards(null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var vineyardList = okResult!.Value as List<Vineyard>;
        vineyardList.Should().HaveCount(2);
    }

    [Test]
    public async Task GetVineyards_WithLikeFilter_PassesFilterToService()
    {
        // Arrange
        var vineyards = new List<Vineyard>
        {
            new Vineyard { Name = "Napa Valley", Count = 10 }
        }.AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetVineyards("Napa"))
            .Returns(vineyards);

        // Act
        var result = await _controller.GetVineyards("Napa");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockWineService.Verify(x => x.GetVineyards("Napa"), Times.Once);
    }

    #endregion

    #region GetBottlesForWine Tests

    [Test]
    public async Task GetBottlesForWine_WithValidWineId_ReturnsBottleList()
    {
        // Arrange
        var bottles = new List<Bottle>
        {
            new Bottle { Id = 1, WineId = 1, BinX = 1, BinY = 1 },
            new Bottle { Id = 2, WineId = 1, BinX = 1, BinY = 2 }
        }.AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetBottles(1, null))
            .Returns(bottles);

        // Act
        var result = await _controller.GetBottlesForWine(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(b => b.WineId == 1).Should().BeTrue();
    }

    #endregion

    #region AddBottle Tests

    [Test]
    public async Task AddBottle_WithValidBottle_ReturnsCreatedBottle()
    {
        // Arrange
        var putBottle = new PutBottle
        {
            WineId = 1,
            StorageId = 1,
            BinX = 5,
            BinY = 3,
            Depth = 2
        };

        var createdBottle = new Bottle
        {
            Id = 1,
            WineId = 1,
            StorageId = 1,
            BinX = 5,
            BinY = 3,
            Depth = 2
        };

        _mockWineService
            .Setup(x => x.AddBottle(putBottle))
            .ReturnsAsync(createdBottle);

        // Act
        var result = await _controller.AddBottle(putBottle);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.WineId.Should().Be(1);
        result.BinX.Should().Be(5);
        _mockWineService.Verify(x => x.AddBottle(putBottle), Times.Once);
    }

    #endregion

    #region PatchBottle Tests

    [Test]
    public async Task PatchBottle_WithValidUpdate_ReturnsUpdatedBottle()
    {
        // Arrange
        var patchBottle = new PatchBottle
        {
            BinX = 7,
            BinY = 4
        };

        var updatedBottle = new Bottle
        {
            Id = 1,
            WineId = 1,
            StorageId = 1,
            BinX = 7,
            BinY = 4,
            Depth = 2
        };

        _mockWineService
            .Setup(x => x.UpdateBottle(1, patchBottle))
            .ReturnsAsync(updatedBottle);

        // Act
        var result = await _controller.PatchBottle(patchBottle, 1);

        // Assert
        result.Should().NotBeNull();
        result.BinX.Should().Be(7);
        result.BinY.Should().Be(4);
        _mockWineService.Verify(x => x.UpdateBottle(1, patchBottle), Times.Once);
    }

    [Test]
    public async Task PatchBottle_MarkAsConsumed_UpdatesBottle()
    {
        // Arrange
        var patchBottle = new PatchBottle
        {
            Consumed = true
        };

        var updatedBottle = new Bottle
        {
            Id = 1,
            WineId = 1,
            StorageId = 1,
            BinX = 1,
            BinY = 1,
            Depth = 1
        };

        _mockWineService
            .Setup(x => x.UpdateBottle(1, patchBottle))
            .ReturnsAsync(updatedBottle);

        // Act
        var result = await _controller.PatchBottle(patchBottle, 1);

        // Assert
        result.Should().NotBeNull();
        _mockWineService.Verify(x => x.UpdateBottle(1, patchBottle), Times.Once);
    }

    #endregion

    #region GetStore Tests

    [Test]
    public async Task GetStore_WithValidId_ReturnsStoreList()
    {
        // Arrange
        var stores = new List<Store>
        {
            new Store { Id = 1, BinX = 1, BinY = 1, Count = 5 },
            new Store { Id = 1, BinX = 1, BinY = 2, Count = 3 }
        }.AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetStoreResult(1))
            .Returns(stores);

        // Act
        var result = await _controller.GetStore(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(s => s.Id == 1).Should().BeTrue();
    }

    #endregion

    #region GetBottlesByBin Tests

    [Test]
    public async Task GetBottlesByBin_WithValidBinId_ParsesAndReturnsBottles()
    {
        // Arrange
        // binId = 1305 means: storeId=1, binX=3, binY=5
        var binId = 1305;
        var expectedStoreId = 1;
        var expectedBinX = 3;
        var expectedBinY = 5;

        var bottles = new List<StoreBottle>
        {
            new StoreBottle
            {
                BottleId = 1,
                WineId = 1,
                BinX = 3,
                BinY = 5,
                Varietal = "Cabernet"
            }
        }.AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetBottlesByStoreAndBin(expectedStoreId, expectedBinX, expectedBinY))
            .Returns(bottles);

        // Act
        var result = await _controller.GetBottlesByBin(binId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].BinX.Should().Be(3);
        result[0].BinY.Should().Be(5);
        _mockWineService.Verify(
            x => x.GetBottlesByStoreAndBin(expectedStoreId, expectedBinX, expectedBinY),
            Times.Once
        );
    }

    [Test]
    public async Task GetBottlesByBin_WithComplexBinId_CorrectlyParsesComponents()
    {
        // Arrange
        // binId = 2917 means: storeId=2, binX=9, binY=17
        var binId = 2917;
        var expectedStoreId = 2;
        var expectedBinX = 9;
        var expectedBinY = 17;

        var bottles = new List<StoreBottle>().AsQueryable().BuildMock();

        _mockWineService
            .Setup(x => x.GetBottlesByStoreAndBin(expectedStoreId, expectedBinX, expectedBinY))
            .Returns(bottles);

        // Act
        await _controller.GetBottlesByBin(binId);

        // Assert
        _mockWineService.Verify(
            x => x.GetBottlesByStoreAndBin(expectedStoreId, expectedBinX, expectedBinY),
            Times.Once
        );
    }

    #endregion
}
