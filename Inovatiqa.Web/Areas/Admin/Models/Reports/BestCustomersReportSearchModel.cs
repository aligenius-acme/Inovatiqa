using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Core;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class BestCustomersReportSearchModel : BaseSearchModel
    {
        #region Ctor

        public BestCustomersReportSearchModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public OrderByEnum OrderBy { get; set; }

        [Display(Name = "Start date")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End date")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Order status")]
        public int OrderStatusId { get; set; }

        [Display(Name = "Payment status")]
        public int PaymentStatusId { get; set; }

        [Display(Name = "Shipping status")]
        public int ShippingStatusId { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }

        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }

        public IList<SelectListItem> AvailableShippingStatuses { get; set; }

        #endregion
    }
}
