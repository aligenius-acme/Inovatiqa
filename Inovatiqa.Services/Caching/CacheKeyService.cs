using Inovatiqa.Core;
using Inovatiqa.Core.Caching;
using Inovatiqa.Services.Caching.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inovatiqa.Services.Caching
{
    public partial class CacheKeyService : ICacheKeyService
    {
        #region Fields

        private const string HASH_ALGORITHM = "SHA1";

        #endregion

        #region Ctor

        #endregion

        #region Utilities

        protected virtual string CreateIdsHash(IEnumerable<int> ids)
        {
            var identifiers = ids.ToList();

            if (!identifiers.Any())
                return string.Empty;

            return HashHelper.CreateHash(Encoding.UTF8.GetBytes(string.Join(", ", identifiers.OrderBy(id => id))), HASH_ALGORITHM);
        }

        //protected virtual object CreateCacheKeyParameters(object parameter)
        //{
        //    return parameter switch
        //    {
        //        null => "null",
        //        IEnumerable<int> ids => CreateIdsHash(ids),
        //        IEnumerable<BaseEntity> entities => CreateIdsHash(entities.Select(e => e.Id)),
        //        BaseEntity entity => entity.Id,
        //        decimal param => param.ToString(CultureInfo.InvariantCulture),
        //        _ => parameter
        //    };
        //}

        protected virtual CacheKey FillCacheKey(CacheKey cacheKey, params object[] keyObjects)
        {
            return new CacheKey(cacheKey, keyObjects);
        }

        #endregion

        #region Methods

        public virtual CacheKey PrepareKey(CacheKey cacheKey, params object[] keyObjects)
        {
            return FillCacheKey(cacheKey, keyObjects);
        }

        public virtual CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] keyObjects)
        {
            var key = FillCacheKey(cacheKey, keyObjects);

            key.CacheTime = InovatiqaDefaults.DefaultCacheTime;

            return key;
        }

        public virtual CacheKey PrepareKeyForShortTermCache(CacheKey cacheKey, params object[] keyObjects)
        {
            var key = FillCacheKey(cacheKey, keyObjects);

            key.CacheTime = InovatiqaDefaults.ShortTermCacheTime;

            return key;
        }

        public virtual string PrepareKeyPrefix(string keyFormatter, params object[] keyObjects)
        {
            return keyObjects?.Any() ?? false
                ? string.Format(keyFormatter, keyObjects.ToArray())
                : keyFormatter;
        }

        #endregion
    }
}
