using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;
using WineApi.Attributes;

namespace WineApi.Filters;

public class FilteringFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var useFiltering = context
            .ActionDescriptor
            .EndpointMetadata
            .OfType<UseFilteringAttribute>()
            .Any();
        
        if ( !useFiltering 
            || context.Result is not ObjectResult objectResult 
            || objectResult.Value is not IQueryable query)
        {
            await next();
            return;
        }

        var filterJson = context.HttpContext.Request.Query["filter"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(filterJson))
        {
            await next();
            return;
        }

        var filterDict = JsonSerializer.Deserialize<Dictionary<string, object>>(filterJson);
        if (filterDict == null)
        {
            await next();
            return;
        }

        //TODO Apply filtering to query

        await next();
    }
}
