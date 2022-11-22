using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Inovatiqa.Core.Caching
{
    public partial class MemoryCacheManager : ILocker, IStaticCacheManager
    {
        #region Fields

        private bool _disposed;

        private readonly IMemoryCache _memoryCache;

        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _prefixes = new ConcurrentDictionary<string, CancellationTokenSource>();
        private static CancellationTokenSource _clearToken = new CancellationTokenSource();

        #endregion

        #region Ctor

        public MemoryCacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        #endregion

        #region Utilities

        private MemoryCacheEntryOptions PrepareEntryOptions(CacheKey key)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(key.CacheTime)
            };

            options.AddExpirationToken(new CancellationChangeToken(_clearToken.Token));
            foreach (var keyPrefix in key.Prefixes.ToList())
            {
                var tokenSource = _prefixes.GetOrAdd(keyPrefix, new CancellationTokenSource());
                options.AddExpirationToken(new CancellationChangeToken(tokenSource.Token));
            }

            return options;
        }

        #endregion

        #region Methods

        public T Get<T>(CacheKey key, Func<T> acquire)
        {
            if (key.CacheTime <= 0)
                return acquire();

            var result = _memoryCache.GetOrCreate(key.Key, entry =>
            {
                entry.SetOptions(PrepareEntryOptions(key));

                return acquire();
            });

            if (result == null)
                Remove(key);

            return result;
        }

        public void Remove(CacheKey key)
        {
            _memoryCache.Remove(key.Key);
        }

        public async Task<T> GetAsync<T>(CacheKey key, Func<Task<T>> acquire)
        {
            if (key.CacheTime <= 0)
                return await acquire();

            var result = await _memoryCache.GetOrCreateAsync(key.Key, async entry =>
             {
                 entry.SetOptions(PrepareEntryOptions(key));

                 return await acquire();
             });

            if (result == null)
                Remove(key);

            return result;
        }

        public void Set(CacheKey key, object data)
        {
            if (key.CacheTime <= 0 || data == null)
                return;

            _memoryCache.Set(key.Key, data, PrepareEntryOptions(key));
        }

        public bool IsSet(CacheKey key)
        {
            return _memoryCache.TryGetValue(key.Key, out _);
        }

        public bool PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
        {
            if (IsSet(new CacheKey(key)))
                return false;

            try
            {
                _memoryCache.Set(key, key, expirationTime);

                action();

                return true;
            }
            finally
            {
                Remove(key);
            }
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public void RemoveByPrefix(string prefix)
        {
            _prefixes.TryRemove(prefix, out var tokenSource);
            tokenSource?.Cancel();
            tokenSource?.Dispose();
        }

        public void Clear()
        {
            _clearToken.Cancel();
            _clearToken.Dispose();

            _clearToken = new CancellationTokenSource();

            foreach (var prefix in _prefixes.Keys.ToList())
            {
                _prefixes.TryRemove(prefix, out var tokenSource);
                tokenSource?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _memoryCache.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}