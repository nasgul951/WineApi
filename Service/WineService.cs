using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Extensions;

namespace WineApi.Service;

public class WineService
{
    private readonly WineContext _db;

    public WineService(WineContext wineContext)
    {
        _db = wineContext;
    }

    public IQueryable<Wine> GetWines(int? id, string? varietal, string? vineyard) {
        return _db.Wines
            .Where(w => w.Bottles.Any(b => b.Consumed == 0))
            .IfThenWhere(id.HasValue, w => w.Wineid == id!.Value)
            .IfThenWhere(!string.IsNullOrWhiteSpace(varietal), w => w.Varietal == varietal)
            .IfThenWhere(!string.IsNullOrWhiteSpace(vineyard), w => w.Vineyard == vineyard)
            .Select(w => new Wine(){
                Id = w.Wineid,
                Vineyard = w.Vineyard,
                Varietal = w.Varietal,
                Label = w.Label,
                Vintage = w.Vintage ?? 0,
                Notes = w.Notes,
                Count = w.Bottles.Where(b => b.Consumed == 0).Count()
            });
    }

    public async Task<Wine> AddWine(Wine model) {
        var wine = new Data.Wine(){
            Vineyard = model.Vineyard,
            Varietal = model.Varietal,
            Label = model.Label,
            Vintage = model.Vintage,
            Notes = model.Notes,
            CreatedDate = DateTime.UtcNow
        };
        _db.Wines.Add(wine);
        await _db.SaveChangesAsync();

        var wines = GetWines(wine.Wineid, null, null);
        return await wines.FirstAsync();
    }

    public async Task<Wine> UpdateWine(Wine model){
        var wine = _db.Wines.First(w => w.Wineid == model.Id);

        if (string.IsNullOrWhiteSpace(model.Varietal))
            wine.Varietal = model.Varietal;

        if (string.IsNullOrWhiteSpace(model.Vineyard))
            wine.Vineyard = model.Vineyard;

        if (string.IsNullOrWhiteSpace(model.Label))
            wine.Label = model.Label;

        if (string.IsNullOrWhiteSpace(model.Notes))
            wine.Notes = model.Notes;

        if (model.Vintage.HasValue)
            wine.Vintage = model.Vintage;

        await _db.SaveChangesAsync();

        return await GetWines(model.Id, null, null).FirstAsync();
    }
    
    public IQueryable<Bottle> GetBottles(int? wineId, int? binId)
    {
        return _db.Bottles
            .Where(b => b.Consumed == 0)
            .IfThenWhere(wineId.HasValue, b => b.Wineid == wineId!.Value)
            //TODO .IfThenWhere(binId.HasValue, b => b.BinX == ? && b.BinY = ?)
            .ToBottleModel();
    }

    public async Task<Bottle> AddBottle(PutBottle model) {
        var bottle = new Data.Bottle() {
            Wineid = model.WineId,
            Storageid = model.StorageId,
            BinX = model.BinX,
            BinY = model.BinY,
            Depth = model.Depth,
            CreatedDate = DateTime.UtcNow
        };
        _db.Bottles.Add(bottle);
        await _db.SaveChangesAsync();

        return await _db.Bottles
            .Where(b => b.Bottleid == bottle.Bottleid)
            .ToBottleModel()
            .FirstAsync();
    }

    public async Task<Bottle> UpdateBottle(int id, PatchBottle model) {
        var bottle = await _db.Bottles.FirstAsync(b => b.Bottleid == id);

        if (model.WineId.HasValue)
            bottle.Wineid = model.WineId.Value;
        if (model.StorageId.HasValue)
            bottle.Storageid = model.StorageId.Value;
        if (model.BinX.HasValue)
            bottle.BinX = model.BinX.Value;
        if (model.BinY.HasValue)
            bottle.BinY = model.BinY.Value;
        if (model.Depth.HasValue)
            bottle.Depth = model.Depth.Value;
        if (model.Consumed.HasValue)
            bottle.Consumed = (sbyte)(model.Consumed.Value ? 1 : 0);
        
        await _db.SaveChangesAsync();

        return await _db.Bottles
            .Where(b => b.Bottleid == bottle.Bottleid)
            .ToBottleModel()
            .FirstAsync();
    }
    
    public IQueryable<Store> GetStoreResult(int storeId)
    {
        return _db.Bottles
            .Where(b => b.Storageid == storeId)
            .GroupBy(b => new { b.BinY, b.BinX })
            .Select(b => new Store()
            {
                Id = storeId * 1000 + (b.Key.BinX * 100) + b.Key.BinY,
                BinX = b.Key.BinX,
                BinY = b.Key.BinY,
                Count = b.Count()
            })
            .OrderBy(s => s.BinY)
            .ThenBy(s => s.BinX);
    }
}