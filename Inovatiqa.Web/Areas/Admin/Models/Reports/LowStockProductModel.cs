using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class LowStockProductModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Product name")]
        public string Name { get; set; }

        public string Attributes { get; set; }

        [Display(Name = "Inventory method")]
        public string ManageInventoryMethod { get; set; }

        [Display(Name = "Stock quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Published")]
        public bool Published { get; set; }

        #endregion
    }
}
