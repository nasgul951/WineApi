namespace WineApi.Tests.Fixtures;

/// <summary>
/// Helper class for building test data objects
/// </summary>
public static class TestDataBuilder
{
    public static WineApi.Data.User CreateTestUser(
        int id = 1,
        string username = "testuser",
        string password = "hashedpassword",
        string salt = "testsalt",
        string? key = null,
        DateTime? keyExpires = null,
        bool isAdmin = false)
    {
        return new WineApi.Data.User
        {
            Id = id,
            Username = username,
            Password = password,
            Salt = salt,
            Key = key,
            KeyExpires = keyExpires,
            LastOn = DateTime.UtcNow.ToString("O"),
            IsAdmin = isAdmin
        };
    }

    public static WineApi.Data.Wine CreateTestWine(
        int id = 1,
        string varietal = "Cabernet Sauvignon",
        string vineyard = "Test Vineyard",
        string label = "Reserve",
        int vintage = 2020,
        string? notes = null)
    {
        return new WineApi.Data.Wine
        {
            Wineid = id,
            Varietal = varietal,
            Vineyard = vineyard,
            Label = label,
            Vintage = vintage,
            Notes = notes,
            TsDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };
    }

    public static WineApi.Data.Bottle CreateTestBottle(
        int id = 1,
        int wineId = 1,
        int storageId = 1,
        int binX = 1,
        int binY = 1,
        int depth = 1,
        DateTime? createdDate = null)
    {
        return new WineApi.Data.Bottle
        {
            Bottleid = id,
            Wineid = wineId,
            Storageid = storageId,
            BinX = binX,
            BinY = binY,
            Depth = depth,
            Consumed = 0,
            TsDate = DateTime.UtcNow,
            CreatedDate = createdDate ?? DateTime.UtcNow
        };
    }

    public static WineApi.Data.Storage CreateTestStorage(
        int id = 1,
        string description = "Test Storage")
    {
        return new WineApi.Data.Storage
        {
            Storageid = id,
            StorageDescription = description,
            TsDate = DateTime.UtcNow
        };
    }
}
