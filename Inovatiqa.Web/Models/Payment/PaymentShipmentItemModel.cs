using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Models.Payment
{
    public class PaymentShipmentItemModel : BaseInovatiqaEntityModel
    {
        #region Ctor

        public PaymentShipmentItemModel()
        {
            AvailableWarehouses = new List<WarehouseInfo>();
        }

        #endregion

        #region Properties

        public int OrderItemId { get; set; }

        public int ProductId { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        public string Sku { get; set; }

        public string AttributeInfo { get; set; }

        public string RentalInfo { get; set; }

        public bool ShipSeparately { get; set; }

        [Display(Name = "Item weight")]
        public string ItemWeight { get; set; }

        [Display(Name = "Item dimensions")]
        public string ItemDimensions { get; set; }

        public int QuantityToAdd { get; set; }

        public int QuantityOrdered { get; set; }

        [Display(Name = "Qty shipped")]
        public int QuantityInThisShipment { get; set; }

        public int QuantityInAllShipments { get; set; }


        public string ShippedFromWarehouse { get; set; }

        public bool AllowToChooseWarehouse { get; set; }

        public List<WarehouseInfo> AvailableWarehouses { get; set; }

        #endregion

        #region Nested Classes

        public class WarehouseInfo : BaseInovatiqaModel
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; }
            public int StockQuantity { get; set; }
            public int ReservedQuantity { get; set; }
            public int PlannedQuantity { get; set; }
        }

        #endregion
    }
}

