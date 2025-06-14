using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using WineApi.Model.Attributes;

/// <summary>
/// Base class for paged requests.
/// Defines properties for pagination and sorting.
/// </summary>
public abstract class PagedRequest
{
    public int Page { get; set; } = 0;
    public int PageSize { get; set; } = 10;
    public string SortField { get; set; } = string.Empty; // The field to sort by, e.g. "Name", "CreatedDate"
    public string SortDirection { get; set; } = "asc"; // or "desc"
    public string Filter { get; set; } = string.Empty; // JSON filter string

    [JsonIgnore]
    public int Skip => Page * PageSize;
    [JsonIgnore]
    public int Take => PageSize;

    [JsonIgnore]
    public bool IsValid => Page >= 0 && PageSize > 0 && PageSize <= 100;
}

/// <summary>
/// Generic paged request class that includes a filter of type TRequest and a response of type TResponse.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class PagedRequest<TRequest, TResponse> : PagedRequest
    where TRequest : new()
{
    public TRequest FilterObject  => string.IsNullOrWhiteSpace(Filter)
        ? new TRequest()
        : JsonSerializer.Deserialize<TRequest>(Filter) ?? new TRequest();

    public PagedRequest()
    {
        // Find property of TResponse wiht DefaultSort attribute
        var sortProp = typeof(TResponse).GetProperties()
            .FirstOrDefault(p => p.GetCustomAttributes(typeof(DefaultSortAttribute), false).Any());

        // TODO: do some default sorting if no property with DefaultSortAttribute is found
        if (sortProp == null)
        {
            throw new InvalidOperationException($"Request type {typeof(TResponse).Name} does not have a property with DefaultSortAttribute.");
        }

        SortField = sortProp.Name;
        SortDirection = sortProp!.GetCustomAttribute<DefaultSortAttribute>()?.Sort ?? "asc";
    }

    public async Task<PagedResponse<TResponse>> BuildResponseAsync(IQueryable<TResponse> query)
    {
        var sortedQuery = query;

        // Apply sorting
        if (SortDirection == "asc")
        {
            sortedQuery = sortedQuery
                .OrderBy(e => EF.Property<object>(e!, SortField.ToPascalCase()));
        }
        else
        {
            sortedQuery = sortedQuery
                .OrderByDescending(e => EF.Property<object>(e!, SortField.ToPascalCase()));
        }

        var totalCount = query.Count();
        var items = await sortedQuery
            .Skip(Skip)
            .Take(Take)
            .ToListAsync();

        return new PagedResponse<TResponse>
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
}
