using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WineApi.Data;
using WineApi.Extensions;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public async Task<List<Wine>> Get(int? id, string? varietal, string? vineyard)
    {
        return await _service.GetWines(id, varietal, vineyard).ToListAsync();
    }

    [HttpPut]
    public async Task<Wine> Put(Wine model) => await _service.AddWine(model);

    [HttpPatch]
    public async Task<Wine> Patch(Wine model) => await _service.UpdateWine(model);

#region Bottles
    [HttpGet("{wineId:int}/bottles")]
    public async Task<List<Bottle>> GetBottles(int wineId) {
        return await _service.GetBottles(wineId, null).ToListAsync();
    }

    [HttpPut("bottles")]
    public async Task<Bottle> PutBottle(PutBottle model) => await _service.AddBottle(model);

    [HttpPatch("bottle/{bottleId:int}")]
    public async Task<Bottle> PatchBottle(PatchBottle model, int bottleId) => await _service.UpdateBottle(bottleId, model);
#endregion

#region Store
    [HttpGet("store/{id}")]
    public async Task<List<Store>> GetStore(int id) {
        return await _service.GetStoreResult(id).ToListAsync();
    }

#endregion


}
