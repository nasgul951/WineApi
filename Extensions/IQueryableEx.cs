using System.Linq.Expressions;

namespace WineApi.Extensions
{
    public static class IQueryableEx 
    {
        public static IQueryable<TSource> IfThenWhere<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
        {
            if(condition)
                source = source.Where(predicate);
            
            return source;
        }
    }
}