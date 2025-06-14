using System.Linq.Expressions;

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
    }
}