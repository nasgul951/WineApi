using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Extensions;
using WineApi.Model.Base;

namespace WineApi.Service;

public class WineService : IWineService
{
    private readonly WineContext _db;

    public WineService(WineContext wineContext)
    {
        _db = wineContext;
    }

    public IQueryable<Wine> GetWines(WineRequest req)
    {
        return _db.Wines
            .IfThenWhere(!req.ShowAll, w => w.Bottles.Any(b => b.Consumed == 0))
            .IfThenWhere(req.Id.HasValue, w => w.Wineid == req.Id!.Value)
            .IfThenWhere(!string.IsNullOrWhiteSpace(req.Varietal), w => w.Varietal == req.Varietal)
            .IfThenWhere(!string.IsNullOrWhiteSpace(req.Vineyard), w => w.Vineyard == req.Vineyard)
            .IfThenWhere(!string.IsNullOrWhiteSpace(req.LabelLike), w => EF.Functions.Like(w.Label, $"%{req.LabelLike}%"))
            .IfThenWhere(req.VintageFrom.HasValue, w => w.Vintage >= req.VintageFrom)
            .IfThenWhere(req.VintageTo.HasValue, w => w.Vintage <= req.VintageTo)
            .ToWineModel();
    }

    public IQueryable<NameSearchResult> GetVarietals(string? like, int? limit)
    {
        var query = _db.Wines
            .Where(w => w.Bottles.Any(b => b.Consumed == 0))
            .IfThenWhere(!string.IsNullOrWhiteSpace(like), w => EF.Functions.Like(w.Varietal, $"%{like}%"))
            .GroupBy(w => w.Varietal);

        if (limit.HasValue)        {
            query = query.Take(limit.Value);
        }

        return query
            .Select(g => new NameSearchResult
            {
                Name = g.Key!,
                Count = g.Count()
            })
            .OrderBy(v => v.Name);
    }

    public IQueryable<NameSearchResult> GetVineyards(string? like, int? limit)
    {
        var query = _db.Wines
            .Where(w => w.Bottles.Any(b => b.Consumed == 0))
            .IfThenWhere(!string.IsNullOrWhiteSpace(like), w => EF.Functions.Like(w.Vineyard, $"%{like}%"))
            .GroupBy(w => w.Vineyard);
        
        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return query.Select(g => new NameSearchResult
        {
            Name = g.Key!,
            Count = g.Count()
        })
        .OrderBy(v => v.Name);
    }

    public IQueryable<NameSearchResult> GetLabels(string? like, int? limit)
    {
        var query = _db.Wines
            .Where(w => w.Bottles.Any(b => b.Consumed == 0))
            .IfThenWhere(!string.IsNullOrWhiteSpace(like), w => EF.Functions.Like(w.Label, $"%{like}%"))
            .GroupBy(w => w.Label);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return query
            .Select(g => new NameSearchResult
            {
                Name = g.Key!,
                Count = g.Count()
            })
            .OrderBy(v => v.Name);
    }

    public async Task<Wine> AddWine(Wine model)
    {
        var wine = new Data.Wine()
        {
            Vineyard = model.Vineyard,
            Varietal = model.Varietal,
            Label = model.Label,
            Vintage = model.Vintage,
            Notes = model.Notes,
            CreatedDate = DateTime.UtcNow
        };
        _db.Wines.Add(wine);
        await _db.SaveChangesAsync();

        var wines = _db.Wines
            .Where(w => w.Wineid == wine.Wineid)
            .ToWineModel();
        return await wines.FirstAsync();
    }

    public async Task<Wine> UpdateWine(int id, WinePatchRequest model)
    {
        var wine = _db.Wines.First(w => w.Wineid == id);
        if (wine == null)
            throw new Exception($"Wine with id {id} not found."); 

        if (!string.IsNullOrWhiteSpace(model.Varietal))
            wine.Varietal = model.Varietal;

        if (!string.IsNullOrWhiteSpace(model.Vineyard))
            wine.Vineyard = model.Vineyard;

        if (!string.IsNullOrWhiteSpace(model.Label))
            wine.Label = model.Label;

        if (!string.IsNullOrWhiteSpace(model.Notes))
            wine.Notes = model.Notes;

        if (model.Vintage.HasValue)
            wine.Vintage = model.Vintage;

        await _db.SaveChangesAsync();

        var wines = _db.Wines
            .Where(w => w.Wineid == wine.Wineid)
            .ToWineModel();
        return await wines.FirstAsync();
    }

    public IQueryable<Bottle> GetBottles(int? wineId, bool showConsumed = false)
    {
        return _db.Bottles
            .Where(b => showConsumed || b.Consumed == 0)
            .IfThenWhere(wineId.HasValue, b => b.Wineid == wineId!.Value)
            .ToBottleModel();
    }

    public async Task<Bottle> AddBottle(PutBottle model)
    {
        var bottle = new Data.Bottle()
        {
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

    public async Task<Bottle> UpdateBottle(int id, PatchBottle model)
    {
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
        if (model.Consumed.HasValue) {
            if (model.Consumed.Value && bottle.Consumed == 0)
            {
                bottle.ConsumedDate = DateTime.UtcNow;
            }

            if (!model.Consumed.Value)
            {
                bottle.ConsumedDate = null;
            }

            bottle.Consumed = (sbyte)(model.Consumed.Value ? 1 : 0);
        }

        await _db.SaveChangesAsync();

        return await _db.Bottles
            .Where(b => b.Bottleid == bottle.Bottleid)
            .ToBottleModel()
            .FirstAsync();
    }

    public IQueryable<StoreCell> GetStoreResult(int storeId)
    {
        Console.WriteLine($"Getting store result for storeId: {storeId}");
        return _db.Bottles
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
            .ThenBy(s => s.BinX);
    }

    public async Task<WineSummary> GetSummary()
    {
        var unconsumed = _db.Bottles.Where(b => b.Consumed == 0);

        var totalBottles = await unconsumed.CountAsync();
        var uniqueWines = await unconsumed.Select(b => b.Wineid).Distinct().CountAsync();
        var uniqueVarietals = await unconsumed.Select(b => b.Wine.Varietal).Distinct().CountAsync();
        var uniqueVineyards = await unconsumed.Select(b => b.Wine.Vineyard).Distinct().CountAsync();

        var lastConsumed = await _db.Bottles
            .Where(b => b.Consumed == 1 && b.ConsumedDate != null)
            .OrderByDescending(b => b.ConsumedDate)
            .Select(b => new LastConsumedWine
            {
                Varietal = b.Wine.Varietal,
                Vineyard = b.Wine.Vineyard,
                Label = b.Wine.Label,
                Vintage = b.Wine.Vintage,
                ConsumedDate = b.ConsumedDate!.Value
            })
            .FirstOrDefaultAsync();

        return new WineSummary
        {
            TotalBottles = totalBottles,
            UniqueWines = uniqueWines,
            UniqueVarietals = uniqueVarietals,
            UniqueVineyards = uniqueVineyards,
            LastConsumed = lastConsumed
        };
    }

    public IQueryable<StoreBottle> GetBottlesByStoreAndBin(int storeId, int binX, int binY)
    {
        return _db.Bottles
            .Where(b => b.Consumed == 0)
            .Where(b => b.Storageid == storeId)
            .Where(b => b.BinY == binY)
            .IfThenWhere(binX > 0, b => b.BinX == binX)
            .Select(b => new StoreBottle()
            {
                BottleId = b.Bottleid,
                WineId = b.Wineid,
                Vineyard = b.Wine.Vineyard,
                Label = b.Wine.Label,
                Varietal = b.Wine.Varietal,
                Vintage = b.Wine.Vintage,
                BinX = b.BinX,
                BinY = b.BinY,
                Depth = b.Depth,
                CreatedDate = b.CreatedDate
            });
    }
}