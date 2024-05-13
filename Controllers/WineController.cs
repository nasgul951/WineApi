using Microsoft.AspNetCore.Mvc;
using WineApi.Data;
using WineApi.Extensions;

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

    public List<Wine> Index(string? varietal, string? vineyard, int? id)
    {
        var wineQuery = _db.Wines
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

        return [.. wineQuery];
    }
}
