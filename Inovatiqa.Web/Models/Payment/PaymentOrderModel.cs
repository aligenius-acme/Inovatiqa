using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Inovatiqa.Core;
using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Web.Models.Payment
{
    public class PaymentOrderModel : BaseInovatiqaEntityModel
    {
        #region Ctor

        public PaymentOrderModel()
        {
            CustomValues = new Dictionary<string, object>();
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<PaymentOrderItemModel>();
            UsedDiscounts = new List<UsedDiscountModel>();
            OrderShipmentSearchModel = new PaymentOrderShipmentSearchModel();
            //OrderNoteSearchModel = new OrderNoteSearchModel();
            BillingAddress = new PaymentAddressModel();
            ShippingAddress = new PaymentAddressModel();
            PickupAddress = new PaymentAddressModel();
        }

        #endregion

        #region Properties

        public bool IsLoggedInAsVendor { get; set; }

        public override int Id { get; set; }

        [Display(Name = "Order GUID")]
        public Guid OrderGuid { get; set; }

        [Display(Name = "Order #")]
        public string CustomOrderNumber { get; set; }

        [Display(Name = "Store")]
        public string StoreName { get; set; }

        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerInfo { get; set; }

        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }
        public string CustomerFullName { get; set; }

        [Display(Name = "Customer IP address")]
        public string CustomerIp { get; set; }

        [Display(Name = "Custom values")]
        public Dictionary<string, object> CustomValues { get; set; }

        [Display(Name = "Affiliate")]
        public int AffiliateId { get; set; }

        [Display(Name = "Affiliate")]
        public string AffiliateName { get; set; }

        [Display(Name = "Used discounts")]
        public IList<UsedDiscountModel> UsedDiscounts { get; set; }

        public bool AllowCustomersToSelectTaxDisplayType { get; set; }
        public TaxDisplayType TaxDisplayType { get; set; }

        [Display(Name = "incl tax")]
        public string OrderSubtotalInclTax { get; set; }

        [Display(Name = "excl tax")]
        public string OrderSubtotalExclTax { get; set; }

        [Display(Name = "incl tax")]
        public string OrderSubTotalDiscountInclTax { get; set; }

        [Display(Name = "excl tax")]
        public string OrderSubTotalDiscountExclTax { get; set; }

        [Display(Name = "incl tax")]
        public string OrderShippingInclTax { get; set; }

        [Display(Name = "excl tax")]
        public string OrderShippingExclTax { get; set; }

        [Display(Name = "incl tax")]
        public string PaymentMethodAdditionalFeeInclTax { get; set; }

        [Display(Name = "excl tax")]
        public string PaymentMethodAdditionalFeeExclTax { get; set; }

        [Display(Name = "Order tax")]
        public string Tax { get; set; }
        public IList<TaxRate> TaxRates { get; set; }
        public bool DisplayTax { get; set; }
        public bool DisplayTaxRates { get; set; }

        [Display(Name = "Order discount")]
        public string OrderTotalDiscount { get; set; }

        [Display(Name = "Reward points")]
        public int RedeemedRewardPoints { get; set; }

        [Display(Name = "Reward points")]
        public string RedeemedRewardPointsAmount { get; set; }

        [Display(Name = "Order total")]
        public string OrderTotal { get; set; }

        [Display(Name = "Refunded amount")]
        public string RefundedAmount { get; set; }

        [Display(Name = "Profit")]
        public string Profit { get; set; }

        [Display(Name = "Order subtotal")]
        public decimal OrderSubtotalInclTaxValue { get; set; }

        [Display(Name = "Order subtotal")]
        public decimal OrderSubtotalExclTaxValue { get; set; }

        [Display(Name = "Order subtotal discount")]
        public decimal OrderSubTotalDiscountInclTaxValue { get; set; }

        [Display(Name = "Order subtotal discount")]
        public decimal OrderSubTotalDiscountExclTaxValue { get; set; }

        [Display(Name = "Order shipping")]
        public decimal OrderShippingInclTaxValue { get; set; }

        [Display(Name = "Order shipping")]
        public decimal OrderShippingExclTaxValue { get; set; }

        [Display(Name = "Payment method additional fee")]
        public decimal PaymentMethodAdditionalFeeInclTaxValue { get; set; }


        [Display(Name = "Payment method additional fee")]
        public decimal PaymentMethodAdditionalFeeExclTaxValue { get; set; }


        [Display(Name = "Order tax")]
        public decimal TaxValue { get; set; }

        [Display(Name = "Order tax rates")]
        public string TaxRatesValue { get; set; }

        [Display(Name = "Order discount")]
        public decimal OrderTotalDiscountValue { get; set; }

        [Display(Name = "Order total")]
        public decimal OrderTotalValue { get; set; }

        [Display(Name = "Recurring payment")]
        public int RecurringPaymentId { get; set; }

        [Display(Name = "Order status")]
        public string OrderStatus { get; set; }

        [Display(Name = "Order status")]
        public int OrderStatusId { get; set; }

        [Display(Name = "Payment status")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Payment status")]
        public int PaymentStatusId { get; set; }

        [Display(Name = "Payment method")]
        public string PaymentMethod { get; set; }

        public bool AllowStoringCreditCardNumber { get; set; }

        [Display(Name = "Card type")]
        public string CardType { get; set; }

        [Display(Name = "Card name")]
        public string CardName { get; set; }

        [Display(Name = "Card number")]
        public string CardNumber { get; set; }

        [Display(Name = "Card CVV2")]
        public string CardCvv2 { get; set; }

        [Display(Name = "Card expiry month")]
        public string CardExpirationMonth { get; set; }

        [Display(Name = "Card expiry year")]
        public string CardExpirationYear { get; set; }

        [Display(Name = "Authorization transaction ID")]
        public string AuthorizationTransactionId { get; set; }

        [Display(Name = "Capture transaction ID")]
        public string CaptureTransactionId { get; set; }

        [Display(Name = "Subscription transaction ID")]
        public string SubscriptionTransactionId { get; set; }

        public bool IsShippable { get; set; }
        public bool PickupInStore { get; set; }

        [Display(Name = "Pickup point address")]
        public PaymentAddressModel PickupAddress { get; set; }
        public string PickupAddressGoogleMapsUrl { get; set; }

        [Display(Name = "Shipping status")]
        public string ShippingStatus { get; set; }

        [Display(Name = "Shipping status")]
        public int ShippingStatusId { get; set; }

        [Display(Name = "Shipping address")]
        public PaymentAddressModel ShippingAddress { get; set; }

        [Display(Name = "Shipping method")]
        public string ShippingMethod { get; set; }
        public string ShippingAddressGoogleMapsUrl { get; set; }
        public bool CanAddNewShipments { get; set; }

        [Display(Name = "Billing address")]
        public PaymentAddressModel BillingAddress { get; set; }

        [Display(Name = "VAT number")]
        public string VatNumber { get; set; }

        public IList<GiftCard> GiftCards { get; set; }

        public bool HasDownloadableProducts { get; set; }
        public IList<PaymentOrderItemModel> Items { get; set; }

        [Display(Name = "Created on")]
        public DateTime CreatedOn { get; set; }

        public string CheckoutAttributeInfo { get; set; }

        [Display(Name = "Display to customer")]
        public bool AddOrderNoteDisplayToCustomer { get; set; }

        [Display(Name = "Note")]
        public string AddOrderNoteMessage { get; set; }
        public bool AddOrderNoteHasDownload { get; set; }

        [Display(Name = "Attached file")]
        [UIHint("Download")]
        public int AddOrderNoteDownloadId { get; set; }

        [Display(Name = "Amount to refund")]
        public decimal AmountToRefund { get; set; }
        public decimal MaxAmountToRefund { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        public bool CanCancelOrder { get; set; }
        public bool CanCapture { get; set; }
        public bool CanMarkOrderAsPaid { get; set; }
        public bool CanRefund { get; set; }
        public bool CanRefundOffline { get; set; }
        public bool CanPartiallyRefund { get; set; }
        public bool CanPartiallyRefundOffline { get; set; }
        public bool CanVoid { get; set; }
        public bool CanVoidOffline { get; set; }

        public PaymentOrderShipmentSearchModel OrderShipmentSearchModel { get; set; }

        //public OrderNoteSearchModel OrderNoteSearchModel { get; set; }

        public IList<Shipment> Shipment { get; set; }

        #endregion

        #region Nested Classes

        public partial class TaxRate 
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial class GiftCard 
        {
            [Display(Name = "Gift card")]
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public partial class UsedDiscountModel 
        {
            public int DiscountId { get; set; }
            public string DiscountName { get; set; }
        }

        #endregion
    }

    public partial class OrderAggreratorModel 
    {
        public string aggregatorprofit { get; set; }
        public string aggregatorshipping { get; set; }
        public string aggregatortax { get; set; }
        public string aggregatortotal { get; set; }
    }
}