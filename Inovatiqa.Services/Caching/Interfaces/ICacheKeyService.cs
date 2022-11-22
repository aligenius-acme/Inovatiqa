using Inovatiqa.Core.Caching;

namespace Inovatiqa.Services.Caching.Interfaces
{
    public partial interface ICacheKeyService
    {
        CacheKey PrepareKey(CacheKey cacheKey, params object[] keyObjects);

        CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] keyObjects);

        CacheKey PrepareKeyForShortTermCache(CacheKey cacheKey, params object[] keyObjects);

        string PrepareKeyPrefix(string keyFormatter, params object[] keyObjects);
    }
}