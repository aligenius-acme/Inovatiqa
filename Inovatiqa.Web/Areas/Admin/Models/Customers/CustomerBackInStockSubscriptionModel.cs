using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerBackInStockSubscriptionModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Store")]
        public string StoreName { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Subscribed on")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}
