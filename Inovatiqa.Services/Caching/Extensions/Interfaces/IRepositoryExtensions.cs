using Inovatiqa.Core;
using Inovatiqa.Core.Caching;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Caching.Interfaces;

namespace Inovatiqa.Services.Caching.Extensions.Interfaces
{
    public static class IRepositoryExtensions
    {
        public static Category ToCachedCategoryGetById(this IRepository<Category> repository, IStaticCacheManager staticCacheManager, ICacheKeyService cacheKeyService, int id, int? cacheTime = null)
        {
            var cacheKey = new CacheKey(GetCacheKey("category", id));
            cacheKey.CacheTime = InovatiqaDefaults.CacheTime;

            if (cacheTime.HasValue)
                cacheKey.CacheTime = cacheTime.Value;
            else
            {
                cacheKey = cacheKeyService.PrepareKeyForDefaultCache(cacheKey);
            }

            return staticCacheManager.Get(cacheKey, () => repository.GetById(id));
        }

        private static string GetCacheKey(string entityName, object id)
        {
            return string.Format(InovatiqaDefaults.InovatiqaEntityCacheKey, entityName, id);
        }
    }
}
