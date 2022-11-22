using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class ShipmentItemSearchModel : BaseSearchModel
    {
        #region Properties

        public int ShipmentId { get; set; }

        #endregion
    }
}