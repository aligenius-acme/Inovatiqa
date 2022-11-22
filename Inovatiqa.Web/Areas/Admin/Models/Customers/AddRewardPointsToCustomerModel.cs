using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class AddRewardPointsToCustomerModel : BaseInovatiqaModel
    {
        #region Ctor

        public AddRewardPointsToCustomerModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int CustomerId { get; set; }

        [Display(Name = "Points")]
        public int Points { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Store")]
        public int StoreId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        [Display(Name = "Activate points immediately")]
        public bool ActivatePointsImmediately { get; set; }

        [Display(Name = "Reward points activation")]
        public int ActivationDelay { get; set; }

        public int ActivationDelayPeriodId { get; set; }

        [Display(Name = "Points validity")]
        [UIHint("Int32Nullable")]
        public int? PointsValidity { get; set; }

        #endregion
    }
}
