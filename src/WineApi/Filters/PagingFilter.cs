using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WineApi.Attributes;
using WineApi.Extensions;
using WineApi.Model.Base;

namespace WineApi.Filters;

public class PagingFilter<T> : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is not ObjectResult objectResult)
        {
            await next();
            return;
        }

        if (objectResult.Value is not IQueryable<T> query)
        {
            await next();
            return;
        }

        var elementType = query.ElementType;

        // TODO make default page size configurable
        var http = context.HttpContext;
        var page = int.TryParse(http.Request.Query["page"], out var p) ? p : 0;
        var pageSize = int.TryParse(http.Request.Query["pageSize"], out var ps) ? ps : 10;
        var sortField = http.Request.Query["sortField"].FirstOrDefault();
        var sortDirection = http.Request.Query["sortDirection"].FirstOrDefault() ?? "asc";

        // No sort field in query, try to get default sort from return type.
        if (sortField == null) {
            var defaultSortProp = elementType
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(DefaultSortAttribute), false).Any());
            if (defaultSortProp == null) {
                defaultSortProp = elementType.GetProperties().First();
            }
            sortField = defaultSortProp.Name;
            sortDirection = defaultSortProp.GetCustomAttribute<DefaultSortAttribute>()?.Sort ?? "asc";
        }

        // Apply sorting
        query = query.ApplySorting(sortField, sortDirection);

        var totalCount = await query.CountAsync();

        var pagedItems = await query
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var pagedResult = new PagedResponse<T>
        {
            Items = pagedItems,
            TotalCount = totalCount
        };
        
        context.Result = new OkObjectResult(pagedResult);

        await next();
    }
}
