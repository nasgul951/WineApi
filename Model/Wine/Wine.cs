public class Wine
{
    public int Id { get; set; }
    public string? Varietal { get; set; }
    public string? Vineyard { get; set; }
    public string? Label { get; set; }
    public int? Vintage { get; set; }
    public string? Notes { get; set; }
    public int Count { get; set; }
}

public class WineRequest : PagedRequest
{
    public int? Id { get; set; }
    public string? Varietal { get; set; }
    public string? Vineyard { get; set; }
    public bool Consumed { get; set; } = false;
}