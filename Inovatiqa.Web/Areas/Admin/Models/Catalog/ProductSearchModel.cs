using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductSearchModel : BaseSearchModel
    {
        #region Ctor

        public ProductSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
            TierPriceSearchModel = new TierPriceSearchModel();
        }

        #endregion

        #region Properties

        public TierPriceSearchModel TierPriceSearchModel { get; set; }
        [Display(Name = "Product name")]
        public string SearchProductName { get; set; }

        [Display(Name = "Category")]
        public int SearchCategoryId { get; set; }

        [Display(Name = "Search subcategories")]
        public bool SearchIncludeSubCategories { get; set; }

        [Display(Name = "Manufacturer")]
        public int SearchManufacturerId { get; set; }

        [Display(Name = "Store")]
        public int SearchStoreId { get; set; }

        [Display(Name = "Vendor")]
        public int SearchVendorId { get; set; }

        [Display(Name = "Warehouse")]
        public int SearchWarehouseId { get; set; }

        [Display(Name = "Product type")]
        public int SearchProductTypeId { get; set; }

        [Display(Name = "Published")]
        public int SearchPublishedId { get; set; }

        [Display(Name = "Go directly to product SKU")]
        public string GoDirectlyToSku { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        public bool AllowVendorsToImportProducts { get; set; }

        public bool HideStoresList { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableWarehouses { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public IList<SelectListItem> AvailableProductTypes { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        #endregion
    }
}
