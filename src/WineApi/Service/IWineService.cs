using WineApi.Model.Attributes.Varietal;
using WineApi.Model.Wine;

namespace WineApi.Service;

public interface IWineService
{
    IQueryable<Wine> GetWines(WineRequest req);
    IQueryable<Varietal> GetVarietals();
    IQueryable<Vineyard> GetVineyards(string? like);
    Task<Wine> AddWine(Wine model);
    Task<Wine> UpdateWine(int id, WinePatchRequest model);
    IQueryable<Bottle> GetBottles(int? wineId, int? binId);
    Task<Bottle> AddBottle(PutBottle model);
    Task<Bottle> UpdateBottle(int id, PatchBottle model);
    IQueryable<Store> GetStoreResult(int storeId);
    IQueryable<StoreBottle> GetBottlesByStoreAndBin(int storeId, int binX, int binY);
}
