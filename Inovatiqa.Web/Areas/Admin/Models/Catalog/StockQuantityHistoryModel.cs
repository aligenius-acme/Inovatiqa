using Inovatiqa.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class StockQuantityHistoryModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Warehouse")]
        public string WarehouseName { get; set; }

        [Display(Name = "Attribute combination")]
        public string AttributeCombination { get; set; }

        [Display(Name = "Quantity adjustment")]
        public int QuantityAdjustment { get; set; }

        [Display(Name = "Stock quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Created On")]
        [UIHint("DecimalNullable")]
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}
