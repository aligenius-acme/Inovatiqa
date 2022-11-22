using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class OrderShipmentSearchModel : BaseSearchModel
    {
        #region Ctor

        public OrderShipmentSearchModel()
        {
            ShipmentItemSearchModel = new ShipmentItemSearchModel();
        }

        #endregion

        #region Properties

        public int OrderId { get; set; }

        public ShipmentItemSearchModel ShipmentItemSearchModel { get; set; }

        #endregion
    }
}