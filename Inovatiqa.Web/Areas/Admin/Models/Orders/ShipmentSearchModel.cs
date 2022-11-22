using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class ShipmentSearchModel : BaseSearchModel
    {
        #region Ctor

        public ShipmentSearchModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            ShipmentItemSearchModel = new ShipmentItemSearchModel();
        }

        #endregion

        #region Properties

        [Display(Name = "Start date")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Tracking number")]
        public string TrackingNumber { get; set; }
        
        public IList<SelectListItem> AvailableCountries { get; set; }

        [Display(Name = "Country")]
        public int CountryId { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }

        [Display(Name = "State / province")]
        public int StateProvinceId { get; set; }

        [Display(Name = "County / region")]
        public string County { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Load not shipped")]
        public bool LoadNotShipped { get; set; }

        [Display(Name = "Load not delivered")]
        public bool LoadNotDelivered { get; set; }

        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        public IList<SelectListItem> AvailableWarehouses { get; set; }

        public ShipmentItemSearchModel ShipmentItemSearchModel { get; set; }

        [Display(Name = "Order #")]
        public int OrderId { get; set; }

        #endregion
    }
}
