using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.ShoppingCart
{
    public partial class ShoppingCartItemModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Store")]
        public string Store { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        public string AttributeInfo { get; set; }

        [Display(Name = "Unit price")]
        public string UnitPrice { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Display(Name = "Total")]
        public string Total { get; set; }

        [Display(Name = "Updated on")]
        public DateTime UpdatedOn { get; set; }

        #endregion
    }
}
