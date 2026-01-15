using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WineApi.Filters;

namespace WineApi.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class UsePagingAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return new WrapperFilter(serviceProvider);
    }

    public sealed class WrapperFilter : IAsyncResultFilter, IFilterMetadata
    {
        private readonly IServiceProvider _serviceProvider;

        public WrapperFilter(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var obj = context.Result is ObjectResult objectResult ? objectResult.Value : null;
            if (obj == null)
            {
                await next();
                return;
            }

            var queryableInterface = obj.GetType().GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>));

            if (queryableInterface == null)
            {
                await next();
                return;
            }

            // Get the element type for the IQueryable<T>
            var elementType = queryableInterface.GetGenericArguments()[0];
            
            // Create the PagingFilter<T>
            var filterType = typeof(PagingFilter<>).MakeGenericType(elementType);
            var filter = (IAsyncResultFilter)_serviceProvider.GetRequiredService(filterType);

            await filter.OnResultExecutionAsync(context, next);
        }
    }
}
