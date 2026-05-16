namespace WineApi.Service;

public interface IStoreService
{
  Task<StoreInventory> GetStoreInfo(int storeId);
}
 