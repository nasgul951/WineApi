using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WineApi.Service;
using Microsoft.AspNetCore.Http.HttpResults;
using WineApi.Attributes;
using WineApi.Model.Base;

namespace WineApi.Controllers;

[Authorize(AuthenticationSchemes = "Token")]
[ApiController]
[Route("[controller]")]
public class WineController : ControllerBase
{
    private readonly ILogger<WineController> _logger;
    private readonly IWineService _service;

    public WineController(ILogger<WineController> logger, IWineService wineService)
    {
        _logger = logger;
        _service = wineService;
    }

    [HttpGet("summary")]
    public async Task<WineSummary> GetSummary() => await _service.GetSummary();

    [HttpGet("query")]
    [UsePaging]
    public IQueryable<Wine> Query([FromQuery] WineRequest req) => _service.GetWines(req);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWine(int id)
    {
        var wine = await _service.GetWines(new WineRequest
        {
            Id = id,
            ShowAll = true
        }).FirstOrDefaultAsync();

        if (wine == null)
        {
            return NotFound();
        }
        return Ok(wine);
    }

    [HttpPost]
    public async Task<Wine> Put(Wine model) => await _service.AddWine(model);

    [HttpPatch("{id:int}")]
    public async Task<Wine> Patch(int id, WinePatchRequest model) => await _service.UpdateWine(id, model);

    [HttpGet("varietals")]  
    public async Task<IActionResult> GetVariatals(string? like, int? limit)
    {
        var query = _service.GetVarietals(like, limit);
        var result = await query.ToListAsync();
        return Ok(result);
    }
    
    [HttpGet("vineyards")]  
    public async Task<IActionResult> GetVineyards(string? like, int? limit)
    {
        var query = _service.GetVineyards(like, limit);
        var result = await query.ToListAsync();
        return Ok(result);
    }

    [HttpGet("labels")]
    public async Task<IActionResult> GetLabels(string? like, int? limit)
    {
        var query = _service.GetLabels(like, limit);
        var result = await query.ToListAsync();
        return Ok(result);
    }

    #region Bottles
    [HttpGet("{wineId:int}/bottles")]
    public async Task<List<Bottle>> GetBottlesForWine(int wineId, bool showConsumed = false)
    {
        return await _service.GetBottles(wineId, showConsumed).ToListAsync();
    }

    [HttpPost("bottles")]
    public async Task<Bottle> AddBottle(PutBottle model) => await _service.AddBottle(model);

    [HttpPatch("bottles/{bottleId:int}")]
    public async Task<Bottle> PatchBottle(PatchBottle model, int bottleId) => await _service.UpdateBottle(bottleId, model);
    #endregion

    #region Store
    // Deprecated endpoint, left in place for backward compatibility. Use GetStoreInventory from StoreController instead.
    [HttpGet("store/{id}")]
    public async Task<List<StoreCell>> GetStore(int id)
    {
        return await _service.GetStoreResult(id).ToListAsync();
    }
    
    // TODO: move this to StoreController
    [HttpGet("store/bin/{binId:int}")]
    public async Task<List<StoreBottle>> GetBottlesByBin(int binId)
    {
        // binId is comprised of
        // storeId * 1000 + binX * 100 + binY
        var storeId = binId / 1000;
        var binX = (binId % 1000) / 100;
        var binY = (binId % 1000) % 100;

        _logger.LogInformation("Retrieving bottles for storeId: {StoreId}, BinX: {BinX}, BinY: {BinY}", storeId, binX, binY);
        return await _service.GetBottlesByStoreAndBin(storeId, binX, binY).ToListAsync();
    }

#endregion
}
