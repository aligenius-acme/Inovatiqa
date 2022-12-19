using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class TierPriceModel : BaseInovatiqaEntityModel
    {
        #region Ctor

        public TierPriceModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableCustomerRoles = new List<SelectListItem>();
            AvailabelCustomer = new List<SelectListItem>();
        }

        #endregion

        #region Properties
        public int ProductId { get; set; }
        public string Category { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; }

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }
        public IList<SelectListItem> AvailabelCustomer { get; set; }
        public string Customer { get; set; }

        [Display(Name = "Customer role")]
        public int CustomerRoleId { get; set; }
        public IList<SelectListItem> AvailableCustomerRoles { get; set; }
        public string CustomerRole { get; set; }


        [Display(Name = "Store")]
        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public string Store { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Rate")]
        public decimal Rate { get; set; }

        [Display(Name = "Start date")]
        [UIHint("DateTimeNullable")]
        public DateTime? StartDateTimeUtc { get; set; }

        [Display(Name = "End date")]
        [UIHint("DateTimeNullable")]
        public DateTime? EndDateTimeUtc { get; set; }

        #endregion
    }
}
