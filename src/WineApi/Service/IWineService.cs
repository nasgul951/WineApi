using WineApi.Model.Base;

namespace WineApi.Service;

public interface IWineService
{
    IQueryable<Wine> GetWines(WineRequest req);
    IQueryable<NameSearchResult> GetVarietals(string? like, int? limit);
    IQueryable<NameSearchResult> GetVineyards(string? like, int? limit);
    IQueryable<NameSearchResult> GetLabels(string? like, int? limit);
    Task<Wine> AddWine(Wine model);
    Task<Wine> UpdateWine(int id, WinePatchRequest model);
    IQueryable<Bottle> GetBottles(int? wineId, bool showConsumed = false);
    Task<Bottle> AddBottle(PutBottle model);
    Task<Bottle> UpdateBottle(int id, PatchBottle model);
    IQueryable<StoreCell> GetStoreResult(int storeId);
    IQueryable<StoreBottle> GetBottlesByStoreAndBin(int storeId, int binX, int binY);
    Task<WineSummary> GetSummary();
}
