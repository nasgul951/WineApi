using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Extensions;
using System.Linq;
using System.Runtime.CompilerServices;

namespace WineApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WineController : ControllerBase
{
    private readonly ILogger<WineController> _logger;
    private readonly WineContext _db;

    public WineController(ILogger<WineController> logger, WineContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public async Task<List<Wine>> Get(int? id, string? varietal, string? vineyard)
    {
        return await GetWines(id, varietal, vineyard).ToListAsync();
    }

    [HttpPut]
    public async Task<Wine> Put(Wine model) {
        var wine = new Data.Wine(){
            Vineyard = model.Vineyard,
            Varietal = model.Varietal,
            Label = model.Label,
            Vintage = model.Vintage,
            Notes = model.Notes,
            CreatedDate = DateTime.UtcNow
        };
        await _db.Wines.AddAsync(wine);

        var wines = GetWines(wine.Wineid, null, null);
        return await wines.FirstAsync();
    }

    [HttpPatch]
    public async Task<Wine> Patch(Wine model){
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

    private IQueryable<Wine> GetWines(int? id, string? varietal, string? vineyard) {
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

#region Bottles
    [HttpGet("bottles")]
    // [HttpGet("{wineId:int}/bottles")]
    public async Task<List<Bottle>> GetBottles(int wineId) {
        return await GetBottlesQuery(wineId, null).ToListAsync();
    }

    [HttpPut("bottles")]
    public async Task<Bottle> PutBottle(PutBottle model) {
        var bottle = new Data.Bottle(){
            Wineid = model.WineId,
            Storageid = model.StorageId,
            BinX = model.BinX,
            BinY = model.BinY,
            Depth = model.Depth,
            CreatedDate = DateTime.UtcNow
        };
        await _db.Bottles.AddAsync(bottle);

        return await GetBottlesQuery(bottle.Bottleid, null).FirstAsync();
    }

    [HttpPatch("bottles")]
    public async Task<Bottle> PatchBottle(PatchBottle model) {
        var bottle = await _db.Bottles.FirstAsync(b => b.Bottleid == model.Id);

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

        return await GetBottlesQuery(model.Id, null).FirstAsync();
    }

    private IQueryable<Bottle> GetBottlesQuery(int? wineId, int? binId) {
        return _db.Bottles
            .Where(b => b.Consumed == 0)
            .IfThenWhere(wineId.HasValue, b => b.Wineid == wineId!.Value)
            //TODO .IfThenWhere(binId.HasValue, b => b.BinX == ? && b.BinY = ?)
            .Select(b => new Bottle(){
                Id = b.Bottleid,
                WineId = b.Wineid,
                StorageId = b.Storageid,
                StorageDescription = b.Storage.StorageDescription,
                BinX = b.BinX,
                BinY = b.BinY,
                Depth = b.Depth,
                CreatedDate = b.CreatedDate
            });
    }
#endregion

#region Store
    [HttpGet("store/{id}")]
    public async Task<List<Store>> GetStore(int id) {
        return await GetStoreQuery(id).ToListAsync();
    }

    private IQueryable<Store> GetStoreQuery(int storeId) {
        return _db.Bottles
            .Where(b => b.Storageid == storeId)
            .GroupBy(b => new { b.BinY, b.BinX })
            .Select(b => new Store() {
                Id = storeId * 1000 + (b.Key.BinX * 100) + b.Key.BinY,
                BinX = b.Key.BinX,
                BinY = b.Key.BinY,
                Count = b.Count()
            })
            .OrderBy(s => s.BinY)
            .ThenBy(s => s.BinX);
    }
#endregion


}
