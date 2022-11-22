using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerOrderModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public override int Id { get; set; }

        [Display(Name = "Order #")]
        public string CustomOrderNumber { get; set; }

        [Display(Name = "Order status")]
        public string OrderStatus { get; set; }

        [Display(Name = "Order status")]
        public int OrderStatusId { get; set; }

        [Display(Name = "Payment status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Shipping status")]
        public string ShippingStatus { get; set; }

        [Display(Name = "Order total")]
        public string OrderTotal { get; set; }

        [Display(Name = "Store")]
        public string StoreName { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}
