using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WineApi.Service;

namespace WineApi.Controllers;

[Authorize(AuthenticationSchemes = "Token")]
[ApiController]
[Route("[controller]")]
public class StoreController : ControllerBase
{
    private readonly ILogger<StoreController> _logger;
    private readonly IStoreService _service;

    public StoreController(ILogger<StoreController> logger, IStoreService storeService)
    {
        _logger = logger;
        _service = storeService;
    }


    [HttpGet("{id}/inventory")]
    public async Task<StoreInventory> GetStoreInventory(int id) => await _service.GetStoreInfo(id);
}