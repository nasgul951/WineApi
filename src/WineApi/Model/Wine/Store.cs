using System.Text.Json.Serialization;

public class StoreInventory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Abbreviation { get; set; } = null!;

    public bool HasTopBin { get; set; }
    
    public bool HasBottomBin { get; set; }
    
    public int Rows { get; set; }
    
    public int Columns { get; set; }
    
    public int TotalBottles { get; set; }

    public List<StoreCell> Cells { get; set; } = new List<StoreCell>();
}

public class StoreCell {
    public int Id { get; set; }

    public int BinX { get; set; }

    public int BinY { get; set; }

    public int Count { get; set; }
}

public class StoreBottle {
    public int BottleId { get; set; }
    
    public int WineId { get; set; }
    
    public string? Vineyard { get; set; }
    
    public string? Label { get; set; }

    public string? Varietal { get; set; }

    public int? Vintage { get; set; }

    public int? BinX { get; set; }

    public int? BinY { get; set; }

    public int Depth { get; set; }

    public DateTime? CreatedDate { get; set; }
}