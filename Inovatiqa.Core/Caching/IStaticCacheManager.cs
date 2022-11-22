using System;
using System.Threading.Tasks;

namespace Inovatiqa.Core.Caching
{
    public interface IStaticCacheManager : IDisposable
    {
        T Get<T>(CacheKey key, Func<T> acquire);

        Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire);

        void Remove(CacheKey key);

        void Set(CacheKey key, object data);

        bool IsSet(CacheKey key);

        void RemoveByPrefix(string prefix);

        void Clear();
    }
}