using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WineApi.Service;

namespace WineApi.Controllers;

[Authorize(AuthenticationSchemes = "Token")]
[ApiController]
[Route("[controller]")]
public class WineController : ControllerBase
{
    private readonly ILogger<WineController> _logger;
    private readonly WineService _service;

    public WineController(ILogger<WineController> logger, WineService wineService)
    {
        _logger = logger;
        _service = wineService;
    }

    [HttpGet]
    public async Task<PagedResponse<Wine>> Get([FromQuery] WineRequest req)
    {
        wineQuery  _service.GetWines(req)
            .OrderByDescending(w => w.Vintage)
            .Skip(req.Skip)
            .Take(req.Take)
            .ToListAsync();
    }

    [HttpPut]
    public async Task<Wine> Put(Wine model) => await _service.AddWine(model);

    [HttpPatch]
    public async Task<Wine> Patch(Wine model) => await _service.UpdateWine(model);

    #region Bottles
    [HttpGet("{wineId:int}/bottles")]
    public async Task<List<Bottle>> GetBottles(int wineId)
    {
        return await _service.GetBottles(wineId, null).ToListAsync();
    }

    [HttpPut("bottles")]
    public async Task<Bottle> PutBottle(PutBottle model) => await _service.AddBottle(model);

    [HttpPatch("bottle/{bottleId:int}")]
    public async Task<Bottle> PatchBottle(PatchBottle model, int bottleId) => await _service.UpdateBottle(bottleId, model);
    #endregion

    #region Store
    [HttpGet("store/{id}")]
    public async Task<List<Store>> GetStore(int id)
    {
        return await _service.GetStoreResult(id).ToListAsync();
    }
    
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
