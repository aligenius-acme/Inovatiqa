using Inovatiqa.Core.Caching;

namespace Inovatiqa.Web.Areas.Admin.Infrastructure.Cache
{
    public static partial class InovatiqaModelCacheDefaults
    {
        public static CacheKey OfficialNewsModelKey => new CacheKey("Inovatiqa.pres.admin.official.news");
        
        public static CacheKey CategoriesListKey => new CacheKey("Inovatiqa.pres.admin.categories.list-{0}", CategoriesListPrefixCacheKey);
        public static string CategoriesListPrefixCacheKey => "Inovatiqa.pres.admin.categories.list";

        public static CacheKey ManufacturersListKey => new CacheKey("Inovatiqa.pres.admin.manufacturers.list-{0}", ManufacturersListPrefixCacheKey);
        public static string ManufacturersListPrefixCacheKey => "Inovatiqa.pres.admin.manufacturers.list";

        public static CacheKey VendorsListKey => new CacheKey("Inovatiqa.pres.admin.vendors.list-{0}", VendorsListPrefixCacheKey);
        public static string VendorsListPrefixCacheKey => "Inovatiqa.pres.admin.vendors.list";
    }
}
