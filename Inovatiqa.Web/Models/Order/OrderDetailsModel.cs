using Inovatiqa.Web.Models.Common;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Order
{
    public partial class OrderDetailsModel
    {
        public OrderDetailsModel()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<OrderItemModel>();
            OrderNotes = new List<OrderNote>();
            Shipments = new List<ShipmentBriefModel>();
            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
            PickupAddress = new AddressModel();
            CustomValues = new Dictionary<string, object>();
            ShipmentDetails = new List<ShipmentDetailsModel>();
        }

        public bool IsTrackView { get; set; }

        public int Id { get; set; }

        public bool PrintMode { get; set; }
        public bool PdfInvoiceDisabled { get; set; }

        public string CustomOrderNumber { get; set; }

        public DateTime CreatedOn { get; set; }

        public int OrderStatusId { get; set; }
        public string OrderStatus { get; set; }

        public bool IsReOrderAllowed { get; set; }

        public bool IsReturnRequestAllowed { get; set; }
        
        public bool IsShippable { get; set; }
        public bool PickupInStore { get; set; }
        public AddressModel PickupAddress { get; set; }
        public int ShippingStatusId { get; set; }
        public string ShippingStatus { get; set; }
        public AddressModel ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public IList<ShipmentBriefModel> Shipments { get; set; }

        public IList<ShipmentDetailsModel> ShipmentDetails { get; set; }

        public AddressModel BillingAddress { get; set; }

        public string VatNumber { get; set; }

        public string PaymentMethod { get; set; }
        public int PaymentMethodStatusId { get; set; }
        public string PaymentMethodStatus { get; set; }
        public bool CanRePostProcessPayment { get; set; }
        public Dictionary<string, object> CustomValues { get; set; }

        public string OrderSubtotal { get; set; }
        public string OrderSubTotalDiscount { get; set; }
        public string OrderShipping { get; set; }
        public string PaymentMethodAdditionalFee { get; set; }
        public string CheckoutAttributeInfo { get; set; }

        public bool PricesIncludeTax { get; set; }
        public bool DisplayTaxShippingInfo { get; set; }
        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }

        public string OrderTotalDiscount { get; set; }
        public int RedeemedRewardPoints { get; set; }
        public string RedeemedRewardPointsAmount { get; set; }
        public string OrderTotal { get; set; }
        
        public IList<GiftCard> GiftCards { get; set; }

        public bool ShowSku { get; set; }
        public IList<OrderItemModel> Items { get; set; }
        
        public IList<OrderNote> OrderNotes { get; set; }

        public bool ShowVendorName { get; set; }

        public string PONumber { get; set; }


        #region Nested Classes

        public partial class OrderItemModel
        {
            public int Id { get; set; }
            public Guid OrderItemGuid { get; set; }
            public string Sku { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string UnitPrice { get; set; }
            public string SubTotal { get; set; }
            public int Quantity { get; set; }
            public string AttributeInfo { get; set; }
            public string RentalInfo { get; set; }
            public string VendorName { get; set; }
            public int DownloadId { get; set; }
            public int LicenseId { get; set; }
            public bool IsShipped { get; set; }
            public string ManufacturerPartNumber { get; set; }
            public IList<Catalog.ManufacturerBriefInfoModel> ProductManufacturers { get; set; }
            public int Bo { get; set; }
            public int Shipped { get; set; }
            public string TrackingNumberUrl { get; set; }
        }

        public partial class TaxRate
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial class GiftCard
        {
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public partial class OrderNote
        {
            public int Id { get; set; }
            public bool HasDownload { get; set; }
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public partial class ShipmentBriefModel
        {
            public int Id { get; set; }
            public string TrackingNumber { get; set; }
            public DateTime? ShippedDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
            public string ShipmentTotalPartialInclTax { get; set; }
            public string ShipmentTotalPartialExclTax { get; set; }
        }

		#endregion
    }
}