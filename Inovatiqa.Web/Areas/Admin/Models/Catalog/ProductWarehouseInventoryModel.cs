using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductWarehouseInventoryModel : BaseInovatiqaModel
    {
        #region Properties

        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        [Display(Name = "Warehouse")]
        public string WarehouseName { get; set; }

        [Display(Name = "Use")]
        public bool WarehouseUsed { get; set; }

        [Display(Name = "Stock qty")]
        public int StockQuantity { get; set; }

        [Display(Name = "Reserved qty")]
        public int ReservedQuantity { get; set; }

        [Display(Name = "Planned quantity")]
        public int PlannedQuantity { get; set; }

        #endregion
    }
}