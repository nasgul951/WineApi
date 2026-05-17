using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Exceptions;

namespace WineApi.Service;

public class StoreService : IStoreService
{
  private readonly WineContext _db;

  public StoreService(WineContext db)
  {
    _db = db;
  }

  public async Task<StoreInventory> GetStoreInfo(int storeId)
  {
    var store = await _db.Storages.FirstOrDefaultAsync(s => s.Storageid == storeId);
    if (store == null)
    {
      throw new InvalidRequestException($"Store with id {storeId} not found");
    }

    return new StoreInventory
    {
      Id = store.Storageid,
      Name = store.StorageDescription ?? "Unnamed Store",
      Abbreviation = store.Abbreviation ?? "XX",
      HasBottomBin = store.HasBottomBin,
      HasTopBin = store.HasTopBin,
      Rows = store.Rows,
      Columns = store.Columns,
      TotalBottles = await _db.Bottles.CountAsync(b => b.Storageid == storeId && b.Consumed == 0),
      Cells = await _db.Bottles
            .Where(b => b.Storageid == storeId)
            .Where(b => b.Consumed == 0)
            .GroupBy(b => new { b.BinY, b.BinX })
            .Select(g => new StoreCell()
            {
                Id = storeId * 1000 + (g.Key.BinX * 100) + g.Key.BinY,
                BinX = g.Key.BinX,
                BinY = g.Key.BinY,
                Count = g.Count()
            })
            .OrderBy(s => s.BinY)
            .ThenBy(s => s.BinX)
            .ToListAsync()
    };
  }
}