using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerRoleProductSearchModel : BaseSearchModel
    {
        #region Ctor

        public CustomerRoleProductSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
            AddProductToCustomerRoleModel = new AddProductToCustomerRoleModel();
        }

        #endregion

        #region Properties

        public AddProductToCustomerRoleModel AddProductToCustomerRoleModel { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

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
