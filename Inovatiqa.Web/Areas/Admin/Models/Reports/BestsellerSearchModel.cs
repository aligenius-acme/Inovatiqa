using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class BestsellerSearchModel : BaseSearchModel
    {
        #region Ctor

        public BestsellerSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [Display(Name = "Start date")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Store")]
        public int StoreId { get; set; }

        [Display(Name = "Order status")]
        public int OrderStatusId { get; set; }

        [Display(Name = "Payment status")]
        public int PaymentStatusId { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Manufacturer")]
        public int ManufacturerId { get; set; }

        [Display(Name = "Billing country")]
        public int BillingCountryId { get; set; }

        [Display(Name = "Vendor")]
        public int VendorId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        #endregion
    }
}
