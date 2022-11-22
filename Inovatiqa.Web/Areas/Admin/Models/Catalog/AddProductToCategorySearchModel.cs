using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class AddProductToCategorySearchModel : BaseSearchModel
    {
        #region Ctor

        public AddProductToCategorySearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Product name")]
        public string SearchProductName { get; set; }

        [Display(Name = "Category")]
        public int SearchCategoryId { get; set; }

        [Display(Name = "Manufacturer")]
        public int SearchManufacturerId { get; set; }

        [Display(Name = "Store")]
        public int SearchStoreId { get; set; }

        [Display(Name = "Vendor")]
        public int SearchVendorId { get; set; }

        [Display(Name = "Product type")]
        public int SearchProductTypeId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public IList<SelectListItem> AvailableProductTypes { get; set; }

        #endregion
    }
}
