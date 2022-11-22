namespace Inovatiqa.Web.Models.Payment
{
    public class PaymentOrderShipmentSearchModel
    {
        #region Ctor

        public PaymentOrderShipmentSearchModel()
        {
            ShipmentItemSearchModel = new PaymentShipmentItemSearchModel();
        }

        #endregion

        #region Properties

        public int OrderId { get; set; }
        public int ShipmentId { get; set; }

        public PaymentShipmentItemSearchModel ShipmentItemSearchModel { get; set; }

        #endregion
    }
}