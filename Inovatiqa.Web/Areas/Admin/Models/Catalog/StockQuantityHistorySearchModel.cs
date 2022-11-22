using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class StockQuantityHistorySearchModel : BaseSearchModel
    {
        #region Ctor

        public StockQuantityHistorySearchModel()
        {
            AvailableWarehouses = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public int ProductId { get; set; }

        [Display(Name = "Warehouse")]
        public int WarehouseId { get; set; }

        public IList<SelectListItem> AvailableWarehouses { get; set; }

        #endregion
    }
}
