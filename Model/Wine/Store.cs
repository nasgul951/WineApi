using System.Text.Json.Serialization;

public class Store {
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

    public int Depth { get; set; }

    public DateTime? CreatedDate { get; set; }
}