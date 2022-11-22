using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Core.Orders;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Areas.Admin.Models.Orders;
using Inovatiqa.Web.Areas.Admin.Models.Reports;
using Inovatiqa.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class OrderModelFactory : IOrderModelFactory
    {
        #region Fields

        private readonly IOrderReportService _orderReportService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IWorkContextService _workContextService;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrencyService _currencyService;
        private readonly IDiscountService _discountService;
        private readonly IVendorService _vendorService;
        private readonly IPictureService _pictureService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressAttributeFormatterService _addressAttributeFormatterService;
        private readonly IEncryptionService _encryptionService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IMeasureService _measureService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;

        #endregion

        #region Ctor

        public OrderModelFactory(IOrderReportService orderReportService,
            IPriceFormatter priceFormatter,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IWorkContextService workContextService,
            IDateTimeHelperService dateTimeHelperService,
            IProductService productService,
            IOrderService orderService,
            IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IPaymentService paymentService,
            ICurrencyService currencyService,
            IDiscountService discountService,
            IVendorService vendorService,
            IPictureService pictureService,
            IReturnRequestService returnRequestService,
            IStateProvinceService stateProvinceService,
            IAddressAttributeFormatterService addressAttributeFormatterService,
            IEncryptionService encryptionService,
            IOrderProcessingService orderProcessingService,
            IMeasureService measureService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IAddressAttributeModelFactory addressAttributeModelFactory)
        {
            _orderReportService = orderReportService;
            _priceFormatter = priceFormatter;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _workContextService = workContextService;
            _dateTimeHelperService = dateTimeHelperService;
            _productService = productService;
            _orderService = orderService;
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _paymentService = paymentService;
            _currencyService = currencyService;
            _discountService = discountService;
            _vendorService = vendorService;
            _pictureService = pictureService;
            _returnRequestService = returnRequestService;
            _stateProvinceService = stateProvinceService;
            _addressAttributeFormatterService = addressAttributeFormatterService;
            _encryptionService = encryptionService;
            _orderProcessingService = orderProcessingService;
            _measureService = measureService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _addressAttributeModelFactory = addressAttributeModelFactory;
        }

        #endregion

        #region Utilities

        protected virtual void PrepareShipmentStatusEventModels(IList<ShipmentStatusEventModel> models, Shipment shipment)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            var shipmentTracker = _shipmentService.GetShipmentTracker(shipment);
            var shipmentEvents = shipmentTracker.GetShipmentEvents(shipment.TrackingNumber);
            if (shipmentEvents == null)
                return;

            foreach (var shipmentEvent in shipmentEvents)
            {
                var shipmentStatusEventModel = new ShipmentStatusEventModel
                {
                    Date = shipmentEvent.Date,
                    EventName = shipmentEvent.EventName,
                    Location = shipmentEvent.Location
                };
                var shipmentEventCountry = _countryService.GetCountryByTwoLetterIsoCode(shipmentEvent.CountryCode);
                shipmentStatusEventModel.Country = shipmentEventCountry != null
                    ? shipmentEventCountry.Name : shipmentEvent.CountryCode;
                models.Add(shipmentStatusEventModel);
            }
        }

        protected virtual void PrepareShipmentItemModel(ShipmentItemModel model, OrderItem orderItem, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (orderItem is null)
                throw new ArgumentNullException(nameof(orderItem));

            if (product is null)
                throw new ArgumentNullException(nameof(product));

            if (orderItem.ProductId != product.Id)
                throw new ArgumentException($"{nameof(orderItem.ProductId)} != {nameof(product.Id)}");

            model.OrderItemId = orderItem.Id;
            model.ProductId = orderItem.ProductId;
            model.ProductName = product.Name;
            model.Sku = _productService.FormatSku(product, orderItem.AttributesXml);
            model.AttributeInfo = orderItem.AttributeDescription;
            model.ShipSeparately = product.ShipSeparately;
            model.QuantityOrdered = orderItem.Quantity;
            model.QuantityInAllShipments = _orderService.GetTotalNumberOfItemsInAllShipment(orderItem);
            model.QuantityToAdd = _orderService.GetTotalNumberOfItemsCanBeAddedToShipment(orderItem);

            var baseWeight = _measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId)?.Name;
            var baseDimension = _measureService.GetMeasureDimensionById(InovatiqaDefaults.BaseDimensionId)?.Name;
            if (orderItem.ItemWeight.HasValue)
                model.ItemWeight = $"{orderItem.ItemWeight:F2} [{baseWeight}]";
            model.ItemDimensions =
                $"{product.Length:F2} x {product.Width:F2} x {product.Height:F2} [{baseDimension}]";

            if (!product.IsRental)
                return;
        }
        protected virtual OrderNoteSearchModel PrepareOrderNoteSearchModel(OrderNoteSearchModel searchModel, Order order)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            searchModel.OrderId = order.Id;

            searchModel.SetGridPageSize();

            return searchModel;
        }
        protected virtual ShipmentItemSearchModel PrepareShipmentItemSearchModel(ShipmentItemSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }
        protected virtual OrderShipmentSearchModel PrepareOrderShipmentSearchModel(OrderShipmentSearchModel searchModel, Order order)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            searchModel.OrderId = order.Id;

            PrepareShipmentItemSearchModel(searchModel.ShipmentItemSearchModel);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        protected virtual void PrepareOrderModelShippingInfo(OrderModel model, Order order)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            model.ShippingStatus = Enum.GetName(typeof(ShippingStatus), order.ShippingStatusId);
            if (order.ShippingStatusId == (int)ShippingStatus.ShippingNotRequired)
                return;

            model.IsShippable = true;
            model.ShippingMethod = order.ShippingMethod;
            model.CanAddNewShipments = _orderService.HasItemsToAddToShipment(order);
            model.PickupInStore = order.PickupInStore;
            if (!order.PickupInStore)
            {
                var shippingAddress = _addressService.GetAddressById(order.ShippingAddressId.Value);
                var shippingCountry = _countryService.GetCountryByAddress(shippingAddress);

                model.ShippingAddress = shippingAddress.ToAddressModel<AddressModel>();
                model.ShippingAddress.CountryName = shippingCountry?.Name;
                model.ShippingAddress.StateProvinceName = _stateProvinceService.GetStateProvinceByAddress(shippingAddress)?.Name;
                PrepareAddressModel(model.ShippingAddress, shippingAddress);
                model.ShippingAddressGoogleMapsUrl = "https://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q=" +
                    $"{WebUtility.UrlEncode(shippingAddress.Address1 + " " + shippingAddress.ZipPostalCode + " " + shippingAddress.City + " " + (shippingCountry?.Name ?? string.Empty))}";
            }
            else
            {
                if (order.PickupAddressId is null)
                    return;

                var pickupAddress = _addressService.GetAddressById(order.PickupAddressId.Value);

                var pickupCountry = _countryService.GetCountryByAddress(pickupAddress);

                model.PickupAddress = pickupAddress.ToAddressModel<AddressModel>();
                model.PickupAddressGoogleMapsUrl = $"https://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q=" +
                    $"{WebUtility.UrlEncode($"{pickupAddress.Address1} {pickupAddress.ZipPostalCode} {pickupAddress.City} {(pickupCountry?.Name ?? string.Empty)}")}";
            }
        }

        protected virtual void PrepareAddressModel(AddressModel model, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.FormattedCustomAddressAttributes = _addressAttributeFormatterService.FormatAttributes(address.CustomAttributes);

            model.FirstNameEnabled = true;
            model.FirstNameRequired = true;
            model.LastNameEnabled = true;
            model.LastNameRequired = true;
            model.EmailEnabled = true;
            model.EmailRequired = true;
            model.CompanyEnabled = InovatiqaDefaults.CompanyEnabled;
            model.CompanyRequired = InovatiqaDefaults.CompanyRequired;
            model.CountryEnabled = InovatiqaDefaults.CountryEnabled;
            model.CountryRequired = InovatiqaDefaults.CountryEnabled;
            model.StateProvinceEnabled = InovatiqaDefaults.StateProvinceEnabled;
            model.CountyEnabled = InovatiqaDefaults.CountyEnabled;
            model.CountyRequired = InovatiqaDefaults.CountyRequired;
            model.CityEnabled = InovatiqaDefaults.CityEnabled;
            model.CityRequired = InovatiqaDefaults.CityRequired;
            model.StreetAddressEnabled = InovatiqaDefaults.StreetAddressEnabled;
            model.StreetAddressRequired = InovatiqaDefaults.StreetAddressRequired;
            model.StreetAddress2Enabled = InovatiqaDefaults.StreetAddress2Enabled;
            model.StreetAddress2Required = InovatiqaDefaults.StreetAddress2Required;
            model.ZipPostalCodeEnabled = InovatiqaDefaults.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = InovatiqaDefaults.ZipPostalCodeRequired;
            model.PhoneEnabled = InovatiqaDefaults.PhoneEnabled;
            model.PhoneRequired = InovatiqaDefaults.PhoneRequired;
            model.FaxEnabled = InovatiqaDefaults.FaxEnabled;
            model.FaxRequired = InovatiqaDefaults.FaxRequired;
        }

        protected virtual void PrepareOrderItemModels(IList<OrderItemModel> models, Order order)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var primaryStoreCurrency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);

            var vendor = _workContextService.CurrentVendor;
            var orderShipment = _shipmentService.GetShipmentsByOrderId(order.Id, true);
            List<ShipmentItem> consolidatedShipmentItems = new List<ShipmentItem>();
            foreach (var shipmentItems in orderShipment)
            {
                consolidatedShipmentItems.AddRange(_shipmentService.GetShipmentItemsByShipmentId(shipmentItems.Id));
            }
            var orderItems = _orderService.GetOrderItems(order.Id, vendorId: vendor?.Id ?? 0);

            foreach (var orderItem in orderItems)
            {
                var product = _productService.GetProductById(orderItem.ProductId);
                //added by hamza for shippment quantity
                var shipped = 0;
                foreach (var ship in consolidatedShipmentItems)
                {
                    if (orderItem.Id == ship.OrderItemId)
                    {
                        shipped = ship.Quantity;
                    }
                }
                var orderItemModel = new OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = product.Name,
                    Quantity = orderItem.Quantity,
                    StockQuantity = orderItem.Product.StockQuantity,
                    ShippedQuantity = shipped,
                    //IsDownload = product.IsDownload,
                    //DownloadCount = orderItem.DownloadCount,
                    //DownloadActivationType = product.DownloadActivationTypeId,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    UnitPriceInclTaxValue = orderItem.UnitPriceInclTax,
                    UnitPriceExclTaxValue = orderItem.UnitPriceExclTax,
                    DiscountInclTaxValue = orderItem.DiscountAmountInclTax,
                    DiscountExclTaxValue = orderItem.DiscountAmountExclTax,
                    SubTotalInclTaxValue = orderItem.PriceInclTax,
                    SubTotalExclTaxValue = orderItem.PriceExclTax,
                    AttributeInfo = orderItem.AttributeDescription
                };

                orderItemModel.Sku = _productService.FormatSku(product, orderItem.AttributesXml);
                orderItemModel.VendorName = _vendorService.GetVendorById(product.VendorId)?.Name;

                var orderItemPicture = _pictureService.GetProductPicture(product, orderItem.AttributesXml);
                orderItemModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(ref orderItemPicture, 75);


                var languageId = InovatiqaDefaults.LanguageId;

                orderItemModel.UnitPriceInclTax = _priceFormatter
                    .FormatPrice(orderItem.UnitPriceInclTax);
                orderItemModel.UnitPriceExclTax = _priceFormatter
                    .FormatPrice(orderItem.UnitPriceExclTax);

                orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax);
                orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax);

                orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.PriceInclTax);
                orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.PriceExclTax);


                PrepareReturnRequestBriefModels(orderItemModel.ReturnRequests, orderItem);


                models.Add(orderItemModel);
            }
        }

        protected virtual void PrepareReturnRequestBriefModels(IList<OrderItemModel.ReturnRequestBriefModel> models, OrderItem orderItem)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var returnRequests = _returnRequestService.SearchReturnRequests(orderItemId: orderItem.Id);
            foreach (var returnRequest in returnRequests)
            {
                models.Add(new OrderItemModel.ReturnRequestBriefModel
                {
                    CustomNumber = returnRequest.CustomNumber,
                    Id = returnRequest.Id
                });
            }
        }

        protected virtual void PrepareOrderModelTotals(OrderModel model, Order order)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var primaryStoreCurrency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);
            var languageId = InovatiqaDefaults.LanguageId;

            model.OrderSubtotalInclTax = _priceFormatter.FormatPrice(order.OrderSubtotalInclTax);
            model.OrderSubtotalExclTax = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;

            var orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountInclTax);
            var orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountExclTax);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            model.OrderShippingInclTax = _priceFormatter.FormatPrice(order.OrderShippingInclTax);
            model.OrderShippingExclTax = _priceFormatter.FormatPrice(order.OrderShippingExclTax);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;


            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPrice(order.PaymentMethodAdditionalFeeInclTax);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPrice(order.PaymentMethodAdditionalFeeExclTax);
            }

            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;

            model.Tax = _priceFormatter.FormatPrice(order.OrderTax);
            
            model.DisplayTaxRates = InovatiqaDefaults.DisplayTaxRates;
            model.DisplayTax = InovatiqaDefaults.DisplayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            if (order.OrderDiscount > 0)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-order.OrderDiscount);
            model.OrderTotalDiscountValue = order.OrderDiscount;

            model.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal);
            model.OrderTotalValue = order.OrderTotal;

            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = _priceFormatter.FormatPrice(order.RefundedAmount);

            var duh = _discountService.GetAllDiscountUsageHistory(orderId: order.Id);
            foreach (var d in duh)
            {
                var discount = _discountService.GetDiscountById(d.DiscountId);

                model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                {
                    DiscountId = d.DiscountId,
                    DiscountName = discount.Name
                });
            }

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
                return;

            var profit = _orderReportService.ProfitReport(orderId: order.Id);
            model.Profit = _priceFormatter.FormatPrice(profit);
        }

        protected virtual void PrepareOrderModelPaymentInfo(OrderModel model, Order order)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

            model.BillingAddress = billingAddress.ToAddressModel<AddressModel>();

            model.BillingAddress.CountryName = _countryService.GetCountryByAddress(billingAddress)?.Name;
            model.BillingAddress.StateProvinceName = _stateProvinceService.GetStateProvinceByAddress(billingAddress)?.Name;

            PrepareAddressModel(model.BillingAddress, billingAddress);

            if (order.AllowStoringCreditCardNumber)
            {
                model.CardType = _encryptionService.DecryptText(order.CardType);

                model.CardName = _encryptionService.DecryptText(order.CardName);

                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);

                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);

                var cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                if (!string.IsNullOrEmpty(cardExpirationMonthDecrypted) && cardExpirationMonthDecrypted != "0")
                    model.CardExpirationMonth = cardExpirationMonthDecrypted;
                var cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                if (!string.IsNullOrEmpty(cardExpirationYearDecrypted) && cardExpirationYearDecrypted != "0")
                    model.CardExpirationYear = cardExpirationYearDecrypted;

                model.AllowStoringCreditCardNumber = true;
            }
            else
            {
                var maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                if (!string.IsNullOrEmpty(maskedCreditCardNumberDecrypted))
                    model.CardNumber = maskedCreditCardNumberDecrypted;
            }

            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;

            if(order.PaymentMethodSystemName == InovatiqaDefaults.PurchaseOrderPaymentName)
                model.PaymentMethod = "Purchase Order";
            else if(order.PaymentMethodSystemName == InovatiqaDefaults.SystemName)
                model.PaymentMethod = "Square";
            else
                 model.PaymentMethod = order.PaymentMethodSystemName;
            model.PaymentStatus = Enum.GetName(typeof(PaymentStatus), order.PaymentStatusId);

            model.CanCancelOrder = _orderProcessingService.CanCancelOrder(order);
            model.CanCapture = _orderProcessingService.CanCapture(order);
            model.CanMarkOrderAsPaid = _orderProcessingService.CanMarkOrderAsPaid(order);
            model.CanRefund = _orderProcessingService.CanRefund(order);
            model.CanRefundOffline = _orderProcessingService.CanRefundOffline(order);
            model.CanPartiallyRefund = _orderProcessingService.CanPartiallyRefund(order, decimal.Zero);
            model.CanPartiallyRefundOffline = _orderProcessingService.CanPartiallyRefundOffline(order, decimal.Zero);
            model.CanVoid = _orderProcessingService.CanVoid(order);
            model.CanVoidOffline = _orderProcessingService.CanVoidOffline(order);

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId)?.CurrencyCode;
            model.MaxAmountToRefund = order.OrderTotal - order.RefundedAmount;

            //recurring payment record
            //model.RecurringPaymentId = _orderService.SearchRecurringPayments(initialOrderId: order.Id, showHidden: true).FirstOrDefault()?.Id ?? 0;
        }

        #endregion

        #region Methods

        public virtual OrderAddressModel PrepareOrderAddressModel(OrderAddressModel model, Order order, Address address)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (address == null)
                throw new ArgumentNullException(nameof(address));

            model.OrderId = order.Id;

            model.Address = address.ToAddressModel<AddressModel>();
            PrepareAddressModel(model.Address, address);

            _baseAdminModelFactory.PrepareCountries(model.Address.AvailableCountries);

            _baseAdminModelFactory.PrepareStatesAndProvinces(model.Address.AvailableStates, model.Address.CountryId);

            _addressAttributeModelFactory.PrepareCustomAddressAttributes(model.Address.CustomAddressAttributes, address);

            return model;
        }

        public virtual OrderSearchModel PrepareOrderSearchModel(OrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = _workContextService.CurrentVendor;

            searchModel.IsLoggedInAsVendor = vendor != null;
            searchModel.BillingPhoneEnabled = InovatiqaDefaults.PhoneEnabled;

            _baseAdminModelFactory.PrepareOrderStatuses(searchModel.AvailableOrderStatuses);
            if (searchModel.AvailableOrderStatuses.Any())
            {
                if (searchModel.OrderStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.OrderStatusIds.Select(id => id.ToString());
                    searchModel.AvailableOrderStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailableOrderStatuses.FirstOrDefault().Selected = true;
            }

            _baseAdminModelFactory.PreparePaymentStatuses(searchModel.AvailablePaymentStatuses);
            if (searchModel.AvailablePaymentStatuses.Any())
            {
                if (searchModel.PaymentStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.PaymentStatusIds.Select(id => id.ToString());
                    searchModel.AvailablePaymentStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailablePaymentStatuses.FirstOrDefault().Selected = true;
            }

            _baseAdminModelFactory.PrepareShippingStatuses(searchModel.AvailableShippingStatuses);
            if (searchModel.AvailableShippingStatuses.Any())
            {
                if (searchModel.ShippingStatusIds?.Any() ?? false)
                {
                    var ids = searchModel.ShippingStatusIds.Select(id => id.ToString());
                    searchModel.AvailableShippingStatuses.Where(statusItem => ids.Contains(statusItem.Value)).ToList()
                        .ForEach(statusItem => statusItem.Selected = true);
                }
                else
                    searchModel.AvailableShippingStatuses.FirstOrDefault().Selected = true;
            }

            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            _baseAdminModelFactory.PrepareWarehouses(searchModel.AvailableWarehouses);

            _baseAdminModelFactory.PreparePaymentMethods(searchModel.AvailablePaymentMethods);

            searchModel.AvailableCountries = _countryService.GetAllCountriesForBilling(showHidden: true)
                .Select(country => new SelectListItem { Text = country.Name, Value = country.Id.ToString() }).ToList();
            searchModel.AvailableCountries.Insert(0, new SelectListItem { Text = "All", Value = "0" });

            searchModel.SetGridPageSize();

            searchModel.HideStoresList = InovatiqaDefaults.HideStoresList;

            return searchModel;
        }

        public virtual OrderAggreratorModel PrepareOrderAggregatorModel(OrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = _workContextService.CurrentVendor;

            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();
            if (vendor != null)
                searchModel.VendorId = vendor.Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);
            var product = _productService.GetProductById(searchModel.ProductId);
            var filterByProductId = product != null && (vendor == null || product.VendorId == vendor.Id)
                ? searchModel.ProductId : 0;

            //prepare additional model data
            var reportSummary = _orderReportService.GetOrderAverageReportLine(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes);

            var profit = _orderReportService.ProfitReport(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                startTimeUtc: startDateValue,
                endTimeUtc: endDateValue,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes);

            //var primaryStoreCurrency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);
            var shippingSum = _priceFormatter
                .FormatPrice(reportSummary.SumShippingExclTax);
            var taxSum = _priceFormatter.FormatPrice(reportSummary.SumTax);
            var totalSum = _priceFormatter.FormatPrice(reportSummary.SumOrders);
            var profitSum = _priceFormatter.FormatPrice(profit);

            var model = new OrderAggreratorModel
            {
                aggregatorprofit = profitSum,
                aggregatorshipping = shippingSum,
                aggregatortax = taxSum,
                aggregatortotal = totalSum
            };

            return model;
        }

        public virtual BestsellerBriefSearchModel PrepareBestsellerBriefSearchModel(BestsellerBriefSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize(5);

            return searchModel;
        }

        public virtual OrderAverageReportListModel PrepareOrderAverageReportListModel(OrderAverageReportSearchModel searchModel)
        {
            var report = new List<OrderAverageReportLineSummary>
            {
                _orderReportService.OrderAverageReport(0, OrderStatus.Submitted),
                _orderReportService.OrderAverageReport(0, OrderStatus.Processing),
                _orderReportService.OrderAverageReport(0, OrderStatus.Complete),
                _orderReportService.OrderAverageReport(0, OrderStatus.Cancelled)
            };

            var pagedList = new PagedList<OrderAverageReportLineSummary>(report, 0, int.MaxValue);

            var model = new OrderAverageReportListModel().PrepareToGrid(searchModel, pagedList, () =>
            {
                return pagedList.Select(reportItem => new OrderAverageReportModel
                {
                    OrderStatus = reportItem.OrderStatus.ToString(),
                    SumTodayOrders = _priceFormatter.FormatPrice(reportItem.SumTodayOrders),
                    SumThisWeekOrders = _priceFormatter.FormatPrice(reportItem.SumThisWeekOrders),
                    SumThisMonthOrders = _priceFormatter.FormatPrice(reportItem.SumThisMonthOrders),
                    SumThisYearOrders = _priceFormatter.FormatPrice(reportItem.SumThisYearOrders),
                    SumAllTimeOrders = _priceFormatter.FormatPrice(reportItem.SumAllTimeOrders)
                });
            });

            return model;
        }

        public virtual OrderIncompleteReportListModel PrepareOrderIncompleteReportListModel(OrderIncompleteReportSearchModel searchModel)
        {
            var orderIncompleteReportModels = new List<OrderIncompleteReportModel>();

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            var orderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<int>().Where(os => os != (int)OrderStatus.Cancelled).ToList();
            var paymentStatuses = new List<int> { (int)PaymentStatus.Pending };
            var psPending = _orderReportService.GetOrderAverageReportLine(psIds: paymentStatuses, osIds: orderStatuses);
            orderIncompleteReportModels.Add(new OrderIncompleteReportModel
            {
                Item = "Total unpaid orders (pending payment status)",
                Count = psPending.CountOrders,
                Total = _priceFormatter.FormatPrice(psPending.SumOrders),
                ViewLink = urlHelper.Action("List", "Order", new
                {
                    orderStatuses = string.Join(",", orderStatuses),
                    paymentStatuses = string.Join(",", paymentStatuses)
                })
            });

            var shippingStatuses = new List<int> { (int)ShippingStatus.NotYetShipped };
            var ssPending = _orderReportService.GetOrderAverageReportLine(osIds: orderStatuses, ssIds: shippingStatuses);
            orderIncompleteReportModels.Add(new OrderIncompleteReportModel
            {
                Item = "Total not yet shipped orders",
                Count = ssPending.CountOrders,
                Total = _priceFormatter.FormatPrice(ssPending.SumOrders),
                ViewLink = urlHelper.Action("List", "Order", new
                {
                    orderStatuses = string.Join(",", orderStatuses),
                    shippingStatuses = string.Join(",", shippingStatuses)
                })
            });

            orderStatuses = new List<int> { (int)OrderStatus.Submitted };
            var osPending = _orderReportService.GetOrderAverageReportLine(osIds: orderStatuses);
            orderIncompleteReportModels.Add(new OrderIncompleteReportModel
            {
                Item = "Total incomplete orders (pending order status)",
                Count = osPending.CountOrders,
                Total = _priceFormatter.FormatPrice(osPending.SumOrders),
                ViewLink = urlHelper.Action("List", "Order", new { orderStatuses = string.Join(",", orderStatuses) })
            });

            var pagedList = new PagedList<OrderIncompleteReportModel>(orderIncompleteReportModels, 0, int.MaxValue);

            var model = new OrderIncompleteReportListModel().PrepareToGrid(searchModel, pagedList, () => pagedList);
            return model;
        }

        public virtual OrderListModel PrepareOrderListModel(OrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var orderStatusIds = (searchModel.OrderStatusIds?.Contains(0) ?? true) ? null : searchModel.OrderStatusIds.ToList();
            var paymentStatusIds = (searchModel.PaymentStatusIds?.Contains(0) ?? true) ? null : searchModel.PaymentStatusIds.ToList();
            var shippingStatusIds = (searchModel.ShippingStatusIds?.Contains(0) ?? true) ? null : searchModel.ShippingStatusIds.ToList();

            var vendor = _workContextService.CurrentVendor;


            if (vendor != null)
                searchModel.VendorId = vendor.Id;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);
            var product = _productService.GetProductById(searchModel.ProductId);
            var filterByProductId = product != null && (vendor == null || product.VendorId == vendor.Id)
                ? searchModel.ProductId : 0;

            var orders = _orderService.SearchOrders(storeId: searchModel.StoreId,
                vendorId: searchModel.VendorId,
                productId: filterByProductId,
                warehouseId: searchModel.WarehouseId,
                paymentMethodSystemName: searchModel.PaymentMethodSystemName,
                createdFromUtc: startDateValue,
                createdToUtc: endDateValue,
                osIds: orderStatusIds,
                psIds: paymentStatusIds,
                ssIds: shippingStatusIds,
                billingPhone: searchModel.BillingPhone,
                billingEmail: searchModel.BillingEmail,
                billingLastName: searchModel.BillingLastName,
                billingCountryId: searchModel.BillingCountryId,
                orderNotes: searchModel.OrderNotes,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new OrderListModel().PrepareToGrid(searchModel, orders, () =>
            {
                return orders.Select(order =>
                {
                    var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

                    var orderModel = new OrderModel
                    {
                        Id = order.Id,
                        OrderStatusId = order.OrderStatusId,
                        PaymentStatusId = order.PaymentStatusId,
                        ShippingStatusId = order.ShippingStatusId,
                        CustomerEmail = billingAddress.Email,
                        CustomerFullName = $"{billingAddress.FirstName} {billingAddress.LastName}",
                        CustomerId = order.CustomerId,
                        CustomOrderNumber = order.CustomOrderNumber
                    };

                    orderModel.CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);

                    orderModel.StoreName = InovatiqaDefaults.CurrentStoreName;
                    orderModel.OrderStatus = Enum.GetName(typeof(OrderStatus), order.OrderStatusId);
                    orderModel.PaymentStatus = Enum.GetName(typeof(PaymentStatus), order.PaymentStatusId);
                    orderModel.ShippingStatus = Enum.GetName(typeof(ShippingStatus), order.ShippingStatusId).Replace("NotYetShipped", "Not yet shipped").Replace("ShippingNotRequired", "Shipping not required").Replace("PartiallyShipped", "Partially shipped");
                    orderModel.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal);

                    return orderModel;
                });
            });

            return model;
        }

        public virtual OrderModel PrepareOrderModel(OrderModel model, Order order, bool excludeProperties = false)
        {
            if (order != null)
            {
                model ??= new OrderModel
                {
                    Id = order.Id,
                    OrderStatusId = order.OrderStatusId,
                    VatNumber = order.VatNumber,
                    CheckoutAttributeInfo = order.CheckoutAttributeDescription
                };

                var customer = _customerService.GetCustomerById(order.CustomerId);

                model.OrderGuid = order.OrderGuid;
                model.CustomOrderNumber = order.CustomOrderNumber;
                model.CustomerIp = order.CustomerIp;
                model.CustomerId = customer.Id;
                model.OrderStatus = Enum.GetName(typeof(OrderStatus), order.OrderStatusId);
                model.StoreName = InovatiqaDefaults.CurrentStoreName;
                model.CustomerInfo = _customerService.IsRegistered(customer) ? customer.Email : "Guest";
                model.CreatedOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
                model.CustomValues = _paymentService.DeserializeCustomValues(order);

                PrepareOrderModelTotals(model, order);

                PrepareOrderItemModels(model.Items, order);
                model.HasDownloadableProducts = model.Items.Any(item => item.IsDownload);

                PrepareOrderModelPaymentInfo(model, order);

                PrepareOrderModelShippingInfo(model, order);

                PrepareOrderShipmentSearchModel(model.OrderShipmentSearchModel, order);
                PrepareOrderNoteSearchModel(model.OrderNoteSearchModel, order);
            }

            var vendor = _workContextService.CurrentVendor;

            model.IsLoggedInAsVendor = vendor != null;
            model.AllowCustomersToSelectTaxDisplayType = InovatiqaDefaults.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = TaxDisplayType.IncludingTax;

            return model;
        }

        public virtual BestsellerBriefListModel PrepareBestsellerBriefListModel(BestsellerBriefSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = _workContextService.CurrentVendor;

            var bestsellers = _orderReportService.BestSellersReport(showHidden: true,
                vendorId: vendor?.Id ?? 0,
                orderBy: searchModel.OrderBy,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new BestsellerBriefListModel().PrepareToGrid(searchModel, bestsellers, () =>
            {
                return bestsellers.Select(bestseller =>
                {
                    var bestsellerModel = new BestsellerModel
                    {
                        ProductId = bestseller.ProductId,
                        TotalQuantity = bestseller.TotalQuantity
                    };

                    bestsellerModel.ProductName = _productService.GetProductById(bestseller.ProductId)?.Name;
                    bestsellerModel.TotalAmount = _priceFormatter.FormatPrice(bestseller.TotalAmount);

                    return bestsellerModel;
                });
            });

            return model;
        }

        public virtual ShipmentModel PrepareShipmentModel(ShipmentModel model, Shipment shipment, Order order,
            bool excludeProperties = false)
        {
            if (shipment != null)
            {
                model ??= shipment.ToShipmentModel<ShipmentModel>();

                model.CanShip = !shipment.ShippedDateUtc.HasValue;
                model.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                var shipmentOrder = _orderService.GetOrderById(shipment.OrderId);

                model.CustomOrderNumber = shipmentOrder.CustomOrderNumber;

                model.ShippedDate = shipment.ShippedDateUtc.HasValue
                    ? _dateTimeHelperService.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc).ToString()
                    : "Not yet";
                model.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                    ? _dateTimeHelperService.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc).ToString()
                    : "Not yet";

                if (shipment.TotalWeight.HasValue)
                    model.TotalWeight =
                        $"{shipment.TotalWeight:F2} [{_measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId)?.Name}]";

                foreach (var item in _shipmentService.GetShipmentItemsByShipmentId(shipment.Id))
                {
                    var orderItem = _orderService.GetOrderItemById(item.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var product = _productService.GetProductById(orderItem.ProductId);

                    var shipmentItemModel = new ShipmentItemModel
                    {
                        Id = item.Id,
                        QuantityInThisShipment = item.Quantity,
                        ShippedFromWarehouse = _shippingService.GetWarehouseById(item.WarehouseId)?.Name
                    };

                    PrepareShipmentItemModel(shipmentItemModel, orderItem, product);

                    model.Items.Add(shipmentItemModel);
                }

                if (!string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    var shipmentTracker = _shipmentService.GetShipmentTracker(shipment);
                    if (shipmentTracker != null)
                    {
                        model.TrackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                        if (InovatiqaDefaults.DisplayShipmentEventsToStoreOwner)
                            PrepareShipmentStatusEventModels(model.ShipmentStatusEvents, shipment);
                    }
                }
            }

            if (shipment != null)
                return model;

            model.OrderId = order.Id;
            model.CustomOrderNumber = order.CustomOrderNumber;

            var vendor = _workContextService.CurrentVendor;

            var orderItems = _orderService.GetOrderItems(order.Id, isShipEnabled: true, vendorId: vendor?.Id ?? 0).ToList();

            foreach (var orderItem in orderItems)
            {
                var shipmentItemModel = new ShipmentItemModel();

                var product = _productService.GetProductById(orderItem.ProductId);

                PrepareShipmentItemModel(shipmentItemModel, orderItem, product);

                if (shipmentItemModel.QuantityToAdd <= 0)
                    continue;

                if (product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock &&
                    product.UseMultipleWarehouses)
                {
                    shipmentItemModel.AllowToChooseWarehouse = true;
                    foreach (var pwi in _productService.GetAllProductWarehouseInventoryRecords(orderItem.ProductId).OrderBy(w => w.WarehouseId).ToList())
                    {
                        if (_productService.GetWarehousesById(pwi.WarehouseId) is Warehouse warehouse)
                        {
                            shipmentItemModel.AvailableWarehouses.Add(new ShipmentItemModel.WarehouseInfo
                            {
                                WarehouseId = warehouse.Id,
                                WarehouseName = warehouse.Name,
                                StockQuantity = pwi.StockQuantity,
                                ReservedQuantity = pwi.ReservedQuantity,
                                PlannedQuantity =
                                    _shipmentService.GetQuantityInShipments(product, warehouse.Id, true, true)
                            });
                        }
                    }
                }
                else
                {
                    var warehouse = _shippingService.GetWarehouseById(product.WarehouseId);
                    if (warehouse != null)
                    {
                        shipmentItemModel.AvailableWarehouses.Add(new ShipmentItemModel.WarehouseInfo
                        {
                            WarehouseId = warehouse.Id,
                            WarehouseName = warehouse.Name,
                            StockQuantity = product.StockQuantity
                        });
                    }
                }

                model.Items.Add(shipmentItemModel);
            }

            return model;
        }

        public virtual OrderShipmentListModel PrepareOrderShipmentListModel(OrderShipmentSearchModel searchModel, Order order)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var vendor = _workContextService.CurrentVendor;

            var shipments = _shipmentService.GetAllShipments(
                orderId: order.Id,
                vendorId: vendor?.Id ?? 0)
                .OrderBy(shipment => shipment.CreatedOnUtc)
                .ToList();

            var pagedShipments = shipments.ToPagedList(searchModel);

            var model = new OrderShipmentListModel().PrepareToGrid(searchModel, pagedShipments, () =>
            {
                return pagedShipments.Select(shipment =>
                {
                    var shipmentModel = shipment.ToShipmentModel<ShipmentModel>();

                    shipmentModel.ShippedDate = shipment.ShippedDateUtc.HasValue
                        ? _dateTimeHelperService.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc).ToString()
                        : "Not yet";
                    shipmentModel.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                        ? _dateTimeHelperService.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc).ToString()
                        : "Not yet";

                    shipmentModel.CanShip = !shipment.ShippedDateUtc.HasValue;
                    shipmentModel.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                    shipmentModel.CustomOrderNumber = order.CustomOrderNumber;

                    var baseWeight = _measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId)?.Name;

                    if (shipment.TotalWeight.HasValue)
                        shipmentModel.TotalWeight = $"{shipment.TotalWeight:F2} [{baseWeight}]";

                    return shipmentModel;
                });
            });

            return model;
        }

        public virtual ShipmentItemListModel PrepareShipmentItemListModel(ShipmentItemSearchModel searchModel, Shipment shipment)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var shipmentItems = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id).ToPagedList(searchModel);

            var model = new ShipmentItemListModel().PrepareToGrid(searchModel, shipmentItems, () =>
            {
                return shipmentItems.Select(item =>
                {
                    var shipmentItemModel = new ShipmentItemModel
                    {
                        Id = item.Id,
                        QuantityInThisShipment = item.Quantity
                    };

                    var orderItem = _orderService.GetOrderItemById(item.OrderItemId);
                    if (orderItem == null)
                        return shipmentItemModel;

                    var product = _productService.GetProductById(orderItem.ProductId);

                    shipmentItemModel.OrderItemId = orderItem.Id;
                    shipmentItemModel.ProductId = orderItem.ProductId;
                    shipmentItemModel.ProductName = product.Name;
                    shipmentItemModel.ShippedFromWarehouse = _shippingService.GetWarehouseById(item.WarehouseId)?.Name;

                    var baseWeight = _measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId)?.Name;
                    var baseDimension = _measureService.GetMeasureDimensionById(InovatiqaDefaults.BaseDimensionId)?.Name;
                    if (orderItem.ItemWeight.HasValue)
                        shipmentItemModel.ItemWeight = $"{orderItem.ItemWeight:F2} [{baseWeight}]";

                    shipmentItemModel.ItemDimensions =
                        $"{product.Length:F2} x {product.Width:F2} x {product.Height:F2} [{baseDimension}]";

                    return shipmentItemModel;
                });
            });

            return model;
        }

        public virtual OrderNoteListModel PrepareOrderNoteListModel(OrderNoteSearchModel searchModel, Order order)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var orderNotes = _orderService.GetOrderNotesByOrderId(order.Id).OrderByDescending(on => on.CreatedOnUtc).ToList().ToPagedList(searchModel);

            var model = new OrderNoteListModel().PrepareToGrid(searchModel, orderNotes, () =>
            {
                return orderNotes.Select(orderNote =>
                {
                    var orderNoteModel = orderNote.ToOrderNoteModel<OrderNoteModel>();

                    orderNoteModel.CreatedOn = _dateTimeHelperService.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc);

                    orderNoteModel.Note = _orderService.FormatOrderNoteText(orderNote);
                    orderNoteModel.DownloadGuid = Guid.Empty;

                    return orderNoteModel;
                });
            });

            return model;
        }

        public virtual ShipmentSearchModel PrepareShipmentSearchModel(ShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareCountries(searchModel.AvailableCountries);

            _baseAdminModelFactory.PrepareStatesAndProvinces(searchModel.AvailableStates, searchModel.CountryId);

            _baseAdminModelFactory.PrepareWarehouses(searchModel.AvailableWarehouses);

            PrepareShipmentItemSearchModel(searchModel.ShipmentItemSearchModel);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ShipmentSearchModel PrepareInvoicedShipmentSearchModel(ShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseAdminModelFactory.PrepareCountries(searchModel.AvailableCountries);

            _baseAdminModelFactory.PrepareStatesAndProvinces(searchModel.AvailableStates, searchModel.CountryId);

            _baseAdminModelFactory.PrepareWarehouses(searchModel.AvailableWarehouses);

            PrepareShipmentItemSearchModel(searchModel.ShipmentItemSearchModel);

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual ShipmentListModel PrepareShipmentListModel(ShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendorId = _workContextService.CurrentVendor?.Id ?? 0;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);

            var shipments = _shipmentService.GetAllShipments(vendorId,
                searchModel.WarehouseId,
                searchModel.CountryId,
                searchModel.StateProvinceId,
                searchModel.County,
                searchModel.City,
                searchModel.TrackingNumber,
                searchModel.LoadNotShipped,
                searchModel.LoadNotDelivered,
                searchModel.OrderId,
                startDateValue,
                endDateValue,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = new ShipmentListModel().PrepareToGrid(searchModel, shipments, () =>
            {
                return shipments.Select(shipment =>
                {
                    var shipmentModel = shipment.ToShipmentModel<ShipmentModel>();

                    shipmentModel.ShippedDate = shipment.ShippedDateUtc.HasValue
                        ? _dateTimeHelperService.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc).ToString()
                        : "Not yet";
                    shipmentModel.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                        ? _dateTimeHelperService.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc).ToString()
                        : "Not yet";

                    shipmentModel.CanShip = !shipment.ShippedDateUtc.HasValue;
                    shipmentModel.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                    var order = _orderService.GetOrderById(shipment.OrderId);

                    shipmentModel.CustomOrderNumber = order.CustomOrderNumber;

                    if (shipment.TotalWeight.HasValue)
                        shipmentModel.TotalWeight = $"{shipment.TotalWeight:F2} [{_measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId)?.Name}]";

                    return shipmentModel;
                });
            });

            return model;
        }

        public virtual ShipmentListModel PrepareInvoicedShipmentListModel(ShipmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendorId = _workContextService.CurrentVendor?.Id ?? 0;
            var startDateValue = !searchModel.StartDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.StartDate.Value, _dateTimeHelperService.CurrentTimeZone);
            var endDateValue = !searchModel.EndDate.HasValue ? null
                : (DateTime?)_dateTimeHelperService.ConvertToUtcTime(searchModel.EndDate.Value, _dateTimeHelperService.CurrentTimeZone).AddDays(1);

            var shipments = _shipmentService.GetAllInvoicedShipments(vendorId,
                searchModel.WarehouseId,
                searchModel.CountryId,
                searchModel.StateProvinceId,
                searchModel.County,
                searchModel.City,
                searchModel.TrackingNumber,
                searchModel.LoadNotShipped,
                searchModel.LoadNotDelivered,
                searchModel.OrderId,
                startDateValue,
                endDateValue,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = new ShipmentListModel().PrepareToGrid(searchModel, shipments, () =>
            {
                return shipments.Select(shipment =>
                {
                    var shipmentModel = shipment.ToShipmentModel<ShipmentModel>();

                    shipmentModel.ShippedDate = shipment.ShippedDateUtc.HasValue
                        ? _dateTimeHelperService.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc).ToString()
                        : "Not yet";
                    shipmentModel.DeliveryDate = shipment.DeliveryDateUtc.HasValue
                        ? _dateTimeHelperService.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc).ToString()
                        : "Not yet";

                    shipmentModel.CanShip = !shipment.ShippedDateUtc.HasValue;
                    shipmentModel.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;

                    var order = _orderService.GetOrderById(shipment.OrderId);

                    shipmentModel.CustomOrderNumber = order.CustomOrderNumber;

                    if (shipment.TotalWeight.HasValue)
                        shipmentModel.TotalWeight = $"{shipment.TotalWeight:F2} [{_measureService.GetMeasureWeightById(InovatiqaDefaults.BaseWeightId)?.Name}]";

                    return shipmentModel;
                });
            });

            return model;
        }

        #endregion
    }
}