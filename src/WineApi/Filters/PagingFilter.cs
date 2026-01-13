using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WineApi.Attributes;
using WineApi.Extensions;

namespace WineApi.Filters;

public class PagingFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var usePaging = context.ActionDescriptor
            .EndpointMetadata
            .OfType<UsePagingAttribute>()
            .Any();
        
        if (!usePaging)
        {
            await next();
            return;
        }

        if (context.Result is not ObjectResult objectResult)
        {
            await next();
            return;
        }

        var query = objectResult.Value as IQueryable<object>;
        if (query == null) {
            throw new Exception("UsePaging expects method to return IQueryable<T>");
        }

        // TODO make default page size configurable
        var http = context.HttpContext;
        var page = int.TryParse(http.Request.Query["page"], out var p) ? p : 0;
        var pageSize = int.TryParse(http.Request.Query["pageSize"], out var ps) ? ps : 10;
        var sortField = http.Request.Query["sortField"].FirstOrDefault();
        var sortDirection = http.Request.Query["sortDirection"].FirstOrDefault() ?? "asc";

        // No sort field in query, try to get default sort from return type.
        if (sortField == null) {
            var defaultSortProp = query.ElementType
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(DefaultSortAttribute), false).Any());
            if (defaultSortProp == null) {
                defaultSortProp = query.ElementType.GetProperties().First();
            }
            sortField = defaultSortProp.Name;
            sortDirection = defaultSortProp.GetCustomAttribute<DefaultSortAttribute>()?.Sort ?? "asc";
        }

        var sortQuery = query.ApplySorting(sortField, sortDirection);

        // Get total
        var totalCount = await query.CountAsync();

        var items = await sortQuery
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var pagedType = typeof(PagedResponse<>).MakeGenericType(query.ElementType);
        var pagedResult = Activator.CreateInstance(pagedType);
        
        pagedType.GetProperty(nameof(PagedResponse<object>.Items))!
            .SetValue(pagedResult, items);
        pagedType.GetProperty(nameof(PagedResponse<object>.TotalCount))!
            .SetValue(pagedResult, totalCount);

        context.Result = new OkObjectResult(pagedResult);

        await next();
    }
}
