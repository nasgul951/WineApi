using System.Text.Json.Serialization;
using WineApi.Attributes;

public class Wine
{
    public int Id { get; set; }

    public string? Varietal { get; set; }

    public string? Vineyard { get; set; }

    public string? Label { get; set; }

    [DefaultSort("asc")]
    public int? Vintage { get; set; }

    public string? Notes { get; set; }

    public int Count { get; set; }

    public static string DefaultSort { get; } = "vintage";
}

public class WineRequest
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("varietal")]
    public string? Varietal { get; set; }

    [JsonPropertyName("vineyard")]
    public string? Vineyard { get; set; }

    [JsonPropertyName("showAll")]
    public bool ShowAll { get; set; } = false;
}

public class WinePatchRequest
{
     [JsonPropertyName("varietal")]
    public string? Varietal { get; set; }

    [JsonPropertyName("vineyard")]
    public string? Vineyard { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("vintage")]
    public int? Vintage { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}