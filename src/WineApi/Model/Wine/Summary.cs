namespace WineApi.Model.Base;

public class WineSummary
{
  public int TotalBottles { get; set; }

  public int UniqueWines { get; set; }

  public int UniqueVarietals { get; set; }

  public int UniqueVineyards { get; set; }

  public LastConsumedWine? LastConsumed { get; set; }
}

public class LastConsumedWine
{
  public string? Varietal { get; set; }
  public string? Vineyard { get; set; }
  public string? Label { get; set; }
  public int? Vintage { get; set; }
  public DateTime ConsumedDate { get; set; }
}