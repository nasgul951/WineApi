using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using WineApi.Model.User;

namespace WineApi.Extensions
{
    public static class IQueryableEx
    {
        public static IQueryable<TSource> IfThenWhere<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
        {
            if (condition)
                source = source.Where(predicate);

            return source;
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortField, string sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortField))
                return query;

            if (sortDirection == "desc")
            {
                return query.OrderByDescending(i => EF.Property<object>(i!, sortField.ToPascalCase()));
            }
            else
            {
                return query.OrderBy(i => EF.Property<object>(i!, sortField.ToPascalCase()));
            }
        }

        public static IQueryable<T> ApplyJsonFilter<T>(this IQueryable<T> query, Dictionary<string, JsonElement>? filters)
        {
            if (filters == null || filters.Count == 0)
                return query;

            var queryObj = Expression.Parameter(typeof(T), "o");
            var finalExpression = default(Expression);

            foreach(var filter in filters)
            {
                var propName = filter.Key;
                var jsonValue = filter.Value;

                var property = typeof(T).GetProperty(propName,
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                    continue;   // Ignore unknown properties

                var typedValue = JsonSerializer.Deserialize(jsonValue.GetRawText(), property.PropertyType);

                var left = Expression.Property(queryObj, property);
                var right = Expression.Constant(typedValue);
                var eq = Expression.Equal(left, right);

                finalExpression = finalExpression == null
                    ? eq
                    : Expression.AndAlso(finalExpression, eq);
            }

            if (finalExpression == null)
                return query;

            var lambda = Expression.Lambda<Func<T, bool>>(finalExpression, queryObj);

            return query.Where(lambda);
        }

        public static IQueryable<Bottle> ToBottleModel(this IQueryable<Data.Bottle> source)
        {
            return source
                .Select(b => new Bottle()
                {
                    Id = b.Bottleid,
                    WineId = b.Wineid,
                    StorageId = b.Storageid,
                    StorageDescription = b.Storage.StorageDescription,
                    BinX = b.BinX,
                    BinY = b.BinY,
                    Depth = b.Depth,
                    CreatedDate = b.CreatedDate
                });
        }

        public static IQueryable<Wine> ToWineModel(this IQueryable<Data.Wine> source)
        {
            return source
                .Select(w => new Wine()
                {
                    Id = w.Wineid,
                    Varietal = w.Varietal,
                    Vineyard = w.Vineyard,
                    Label = w.Label,
                    Vintage = w.Vintage,
                    Notes = w.Notes,
                    Count = w.Bottles.Count(b => b.Consumed == 0)
                });
        }

        public static IQueryable<UserDto> ToUserDto(this IQueryable<Data.User> source)
        {
            return source
                .Select(u => new UserDto()
                {
                    Id = u.Id,
                    Username = u.Username,
                    LastOn = u.LastOn,
                    IsAdmin = u.IsAdmin,
                });
        }
    }
}