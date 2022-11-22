using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class OrderSearchModel : BaseSearchModel
    {
        #region Ctor

        public OrderSearchModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailablePaymentMethods = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
            OrderStatusIds = new List<int>();
            PaymentStatusIds = new List<int>();
            ShippingStatusIds = new List<int>();
        }

        #endregion

        #region Properties

        [Display(Name = "Start date")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Order statuses")]
        public IList<int> OrderStatusIds { get; set; }

        [Display(Name = "Payment statuses")]
        public IList<int> PaymentStatusIds { get; set; }

        [Display(Name = "Shipping statuses")]
        public IList<int> ShippingStatusIds { get; set; }

        [Display(Name = "Payment method")]
        public string PaymentMethodSystemName { get; set; }

        [Display(Name = "Store")]
        public int StoreId { get; set; }

        [Display(Name = "Vendor")]
        public int VendorId { get; set; }

        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Billing email address")]
        public string BillingEmail { get; set; }

        [Display(Name = "Billing phone number")]
        public string BillingPhone { get; set; }

        public bool BillingPhoneEnabled { get; set; }

        [Display(Name = "Billing last name")]
        public string BillingLastName { get; set; }

        [Display(Name = "Billing country")]
        public int BillingCountryId { get; set; }

        [Display(Name = "Order notes")]
        public string OrderNotes { get; set; }

        [Display(Name = "Go directly to order #")]
        public string GoDirectlyToCustomOrderNumber { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }

        public IList<SelectListItem> AvailableShippingStatuses { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public IList<SelectListItem> AvailableWarehouses { get; set; }

        public IList<SelectListItem> AvailablePaymentMethods { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}