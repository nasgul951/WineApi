public class Bottle {
    public int Id { get; set; }
    public int WineId { get; set; }
    public int StorageId {get; set; }
    public string? StorageDescription { get; set; }
    public int BinX {get; set;}
    public int BinY { get; set; }
    public int Depth { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public class PutBottle {
    public int WineId { get; set; }
    public int StorageId { get; set; }
    public int BinX { get; set; }
    public int BinY { get; set; }
    public int Depth { get; set; }
}

public class PatchBottle {
    public int Id { get; set; }
    public int? WineId { get; set; }
    public int? StorageId { get; set; }
    public int? BinX { get; set; }
    public int? BinY { get; set; }
    public int? Depth { get; set; }
    public bool? Consumed { get; set; }
}