using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WineApi.Service;
using Microsoft.AspNetCore.Http.HttpResults;

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

    [HttpGet("query")]
    public async Task<IActionResult> Get([FromQuery] PagedRequest<WineRequest, Wine> req)
    {
        var q = _service.GetWines(req.FilterObject);
        var response = await req.BuildResponseAsync(q);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetWine(int id)
    {
        var wine = await _service.GetWines(new WineRequest { Id = id })
            .FirstOrDefaultAsync();
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
    public async Task<IActionResult> GetVariatals([FromQuery] WineRequest req)
    {
        var query = _service.GetVarietals();
        var result = await query.ToListAsync();
        return Ok(result);
    }
    
    #region Bottles
    [HttpGet("{wineId:int}/bottles")]
    public async Task<List<Bottle>> GetBottlesForWine(int wineId)
    {
        return await _service.GetBottles(wineId, null).ToListAsync();
    }

    [HttpPost("bottles")]
    public async Task<Bottle> AddBottle(PutBottle model) => await _service.AddBottle(model);

    [HttpPatch("bottles/{bottleId:int}")]
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
