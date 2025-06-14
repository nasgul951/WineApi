using Microsoft.EntityFrameworkCore;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }

    public static async Task<PagedResponse<T>> BuildResponse(IQueryable<T> query)
    {
        return new PagedResponse<T>
        {
            Items = await query.ToListAsync(),
            TotalCount = await query.CountAsync()
        };
    }
}