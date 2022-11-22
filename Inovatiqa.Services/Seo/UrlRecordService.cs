using Inovatiqa.Core;
using Inovatiqa.Core.Caching;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Caching.Extensions.Interfaces;
using Inovatiqa.Services.Caching.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using System;
using System.Linq;

namespace Inovatiqa.Services.Seo
{
    public partial class UrlRecordService : IUrlRecordService
    {
        #region Fields

        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICacheKeyService _cacheKeyService;

        #endregion

        #region Ctor

        public UrlRecordService(IRepository<UrlRecord> urlRecordRepository,
            IStaticCacheManager staticCacheManager,
            ICacheKeyService cacheKeyService)
        {
            _urlRecordRepository = urlRecordRepository;
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;
        }

        #endregion

        #region Methods

        public virtual string GetActiveSlug(int entityId, string entityName, int languageId)
        {
            //var query = from ur in _urlRecordRepository.Query()
            //            where ur.EntityId == entityId &&
            //                  ur.EntityName == entityName &&
            //                  ur.LanguageId == languageId &&
            //                  ur.IsActive
            //            orderby ur.Id descending
            //            select ur.Slug;
            //return query.FirstOrDefault();


            //var key = InovatiqaDefaults.UrlRecordActiveByIdNameLanguageCacheKey;

            var key = _cacheKeyService.PrepareKeyForDefaultCache(InovatiqaDefaults.UrlRecordActiveByIdNameLanguageCacheKey, entityId, entityName, languageId);


            if (InovatiqaDefaults.LoadAllUrlRecordsOnStartup)
            {
                return _staticCacheManager.Get(key, () =>
                {
                    var source = GetAllUrlRecords();
                    var urlRecords = from ur in source
                                     where ur.EntityId == entityId &&
                                           ur.EntityName == entityName &&
                                           ur.LanguageId == languageId &&
                                           ur.IsActive
                                     orderby ur.Id descending
                                     select ur.Slug;
                    var slug = urlRecords.FirstOrDefault() ?? string.Empty;

                    return slug;
                });
            }

            var query = from ur in _urlRecordRepository.Query()
                        where ur.EntityId == entityId &&
                              ur.EntityName == entityName &&
                              ur.LanguageId == languageId &&
                              ur.IsActive
                        orderby ur.Id descending
                        select ur.Slug;

            var rezSlug = query.ToCachedFirstOrDefault(key, _staticCacheManager) ?? string.Empty;

            return rezSlug;
        }

        public virtual UrlRecord GetBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return null;

            var query = from ur in _urlRecordRepository.Query()
                        where ur.Slug == slug
                        orderby ur.IsActive descending, ur.Id
                        select ur;

            var urlRecord = query.FirstOrDefault();

            return urlRecord;
        }

        public virtual IPagedList<UrlRecord> GetAllUrlRecords(
            string slug = "", int? languageId = null, bool? isActive = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _urlRecordRepository.Query();
            query = query.OrderBy(ur => ur.Slug);

            var urlRecords = query
                .ToCachedList(InovatiqaDefaults.UrlRecordAllCacheKey, _staticCacheManager)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(slug))
                urlRecords = urlRecords.Where(ur => ur.Slug.Contains(slug));

            if (languageId.HasValue)
                urlRecords = urlRecords.Where(ur => ur.LanguageId == languageId);

            if (isActive.HasValue)
                urlRecords = urlRecords.Where(ur => ur.IsActive == isActive);

            var result = urlRecords.ToList();

            return new PagedList<UrlRecord>(result, pageIndex, pageSize);
        }

        public virtual void UpdateUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException(nameof(urlRecord));

            _urlRecordRepository.Update(urlRecord);

            //event notification
            //_eventPublisher.EntityUpdated(urlRecord);
        }

        public virtual void InsertUrlRecord(UrlRecord urlRecord)
        {
            if (urlRecord == null)
                throw new ArgumentNullException(nameof(urlRecord));

            _urlRecordRepository.Insert(urlRecord);

            //event notification
            //_eventPublisher.EntityInserted(urlRecord);
        }

        public virtual void SaveManufacturerSlug(Manufacturer entity, string slug, int languageId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityId = entity.Id;
            var entityName = entity.GetType().Name;

            var query = from ur in _urlRecordRepository.Query()
                        where ur.EntityId == entityId &&
                              ur.EntityName == entityName &&
                              ur.LanguageId == languageId
                        orderby ur.Id descending
                        select ur;
            var allUrlRecords = query.ToList();
            var activeUrlRecord = allUrlRecords.FirstOrDefault(x => x.IsActive);
            UrlRecord nonActiveRecordWithSpecifiedSlug;

            if (activeUrlRecord == null && !string.IsNullOrWhiteSpace(slug))
            {
                nonActiveRecordWithSpecifiedSlug = allUrlRecords
                    .FirstOrDefault(
                        x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                if (nonActiveRecordWithSpecifiedSlug != null)
                {
                    nonActiveRecordWithSpecifiedSlug.IsActive = true;
                    UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);
                }
                else
                {
                    var urlRecord = new UrlRecord
                    {
                        EntityId = entityId,
                        EntityName = entityName,
                        Slug = slug,
                        LanguageId = languageId,
                        IsActive = true
                    };
                    InsertUrlRecord(urlRecord);
                }
            }

            if (activeUrlRecord != null && string.IsNullOrWhiteSpace(slug))
            {
                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }

            if (activeUrlRecord == null || string.IsNullOrWhiteSpace(slug))
                return;

            if (activeUrlRecord.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase))
                return;

            nonActiveRecordWithSpecifiedSlug = allUrlRecords
                .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
            if (nonActiveRecordWithSpecifiedSlug != null)
            {
                nonActiveRecordWithSpecifiedSlug.IsActive = true;
                UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);

                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }
            else
            {
                var urlRecord = new UrlRecord
                {
                    EntityId = entityId,
                    EntityName = entityName,
                    Slug = slug,
                    LanguageId = languageId,
                    IsActive = true
                };
                InsertUrlRecord(urlRecord);

                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }
        }

        public virtual void SaveVendorSlug(Vendor entity, string slug, int languageId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityId = entity.Id;
            var entityName = entity.GetType().Name;

            var query = from ur in _urlRecordRepository.Query()
                        where ur.EntityId == entityId &&
                              ur.EntityName == entityName &&
                              ur.LanguageId == languageId
                        orderby ur.Id descending
                        select ur;
            var allUrlRecords = query.ToList();
            var activeUrlRecord = allUrlRecords.FirstOrDefault(x => x.IsActive);
            UrlRecord nonActiveRecordWithSpecifiedSlug;

            if (activeUrlRecord == null && !string.IsNullOrWhiteSpace(slug))
            {
                nonActiveRecordWithSpecifiedSlug = allUrlRecords
                    .FirstOrDefault(
                        x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
                if (nonActiveRecordWithSpecifiedSlug != null)
                {
                    nonActiveRecordWithSpecifiedSlug.IsActive = true;
                    UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);
                }
                else
                {
                    var urlRecord = new UrlRecord
                    {
                        EntityId = entityId,
                        EntityName = entityName,
                        Slug = slug,
                        LanguageId = languageId,
                        IsActive = true
                    };
                    InsertUrlRecord(urlRecord);
                }
            }

            if (activeUrlRecord != null && string.IsNullOrWhiteSpace(slug))
            {
                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }

            if (activeUrlRecord == null || string.IsNullOrWhiteSpace(slug))
                return;

            if (activeUrlRecord.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase))
                return;

            nonActiveRecordWithSpecifiedSlug = allUrlRecords
                .FirstOrDefault(x => x.Slug.Equals(slug, StringComparison.InvariantCultureIgnoreCase) && !x.IsActive);
            if (nonActiveRecordWithSpecifiedSlug != null)
            {
                nonActiveRecordWithSpecifiedSlug.IsActive = true;
                UpdateUrlRecord(nonActiveRecordWithSpecifiedSlug);

                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }
            else
            {
                var urlRecord = new UrlRecord
                {
                    EntityId = entityId,
                    EntityName = entityName,
                    Slug = slug,
                    LanguageId = languageId,
                    IsActive = true
                };
                InsertUrlRecord(urlRecord);

                activeUrlRecord.IsActive = false;
                UpdateUrlRecord(activeUrlRecord);
            }
        }

        #endregion
    }
}
