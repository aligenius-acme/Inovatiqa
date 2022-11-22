using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Core.Caching
{
    public partial class CacheKey
    {
        #region Fields

        protected string _keyFormat = "";

        #endregion

        #region Ctor

        public CacheKey(CacheKey cacheKey,  params object[] keyObjects)
        {
            Init(cacheKey.Key, cacheKey.CacheTime, cacheKey.Prefixes.ToArray());

            if(!keyObjects.Any())
                return;

            Key = string.Format(cacheKey.Key, keyObjects.ToArray());

            for (var i = 0; i < Prefixes.Count; i++)
                Prefixes[i] = string.Format(Prefixes[i], keyObjects.ToArray());
        }

        public CacheKey(string cacheKey, int? cacheTime = null, params string[] prefixes)
        {
            Init(cacheKey, cacheTime, prefixes);
        }

        public CacheKey(string cacheKey, params string[] prefixes)
        {
            Init(cacheKey, null, prefixes);
        }

        #endregion

        #region Utilities

        protected void Init(string cacheKey, int? cacheTime = null, params string[] prefixes)
        {
            Key = cacheKey;

            _keyFormat = cacheKey;

            if (cacheTime.HasValue)
                CacheTime = cacheTime.Value;

            Prefixes.AddRange(prefixes.Where(prefix=> !string.IsNullOrEmpty(prefix)));
        }

        #endregion

        public string Key { get; protected set; }

        public List<string> Prefixes { get; protected set; } = new List<string>();

        public int CacheTime { get; set; } = InovatiqaDefaults.CacheTime;
    }
}
