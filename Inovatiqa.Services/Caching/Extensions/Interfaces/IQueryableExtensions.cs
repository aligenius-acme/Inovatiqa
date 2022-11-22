using Inovatiqa.Core.Caching;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Caching.Extensions.Interfaces
{
    public static class IQueryableExtensions
    {
        public static IList<T> ToCachedList<T>(this IQueryable<T> query, CacheKey cacheKey, IStaticCacheManager cacheManager)
        {
            return cacheKey == null ? query.ToList() : cacheManager.Get(cacheKey, query.ToList);
        }

        public static T[] ToCachedArray<T>(this IQueryable<T> query, CacheKey cacheKey, IStaticCacheManager cacheManager)
        {
            return cacheKey == null ? query.ToArray() : cacheManager.Get(cacheKey, query.ToArray);
        }

        public static T ToCachedFirstOrDefault<T>(this IQueryable<T> query, CacheKey cacheKey, IStaticCacheManager cacheManager)
        {
            return cacheKey == null
                ? query.FirstOrDefault()
                : cacheManager.Get(cacheKey, query.FirstOrDefault);
        }

        public static T ToCachedSingle<T>(this IQueryable<T> query, CacheKey cacheKey, IStaticCacheManager cacheManager)
        {
            return cacheKey == null
                ? query.Single()
                : cacheManager.Get(cacheKey, query.Single);
        }

        public static bool ToCachedAny<T>(this IQueryable<T> query, CacheKey cacheKey, IStaticCacheManager cacheManager)
        {
            return cacheKey == null
                ? query.Any()
                : cacheManager.Get(cacheKey, query.Any);
        }

        public static int ToCachedCount<T>(this IQueryable<T> query, CacheKey cacheKey, IStaticCacheManager cacheManager)
        {
            return cacheKey == null
                ? query.Count()
                : cacheManager.Get(cacheKey, query.Count);
        }
    }
}
