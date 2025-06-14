using System.Text.Json.Serialization;

public class PagedRequest
{
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 10;

    [JsonIgnore]
    public int Skip => Page * PageSize;
    [JsonIgnore]
    public int Take => PageSize;

    [JsonIgnore]
    public bool IsValid => Page >= 0 && PageSize > 0 && PageSize <= 100;
}