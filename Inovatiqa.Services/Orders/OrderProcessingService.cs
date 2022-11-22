using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Discounts.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Orders
{
    public partial class OrderProcessingService : IOrderProcessingService
    {
        #region Fields

        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICustomerService _customerService;
        private readonly IPaymentService _paymentService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICheckoutAttributeFormatterService _checkoutAttributeFormatterService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWebHelper _webHelper;
        private readonly IEncryptionService _encryptionService;
        private readonly IOrderService _orderService;
        private readonly ICustomNumberFormatterService _customNumberFormatterService;
        private readonly IProductAttributeFormatterService _productAttributeFormatterService;
        private readonly IShippingService _shippingService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContextService _workContextService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IPdfService _pdfService;
        private readonly IVendorService _vendorService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly ILoggerService _loggerService;
        private readonly IShipmentService _shipmentService;
        private readonly IDiscountService _discountService;

        #endregion

        #region Ctor

        public OrderProcessingService(IOrderTotalCalculationService orderTotalCalculationService,
            ICustomerService customerService,
            IPaymentService paymentService,
            IAddressService addressService,
            ICountryService countryService,
            IGenericAttributeService genericAttributeService,
            ICheckoutAttributeFormatterService checkoutAttributeFormatterService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IPriceFormatter priceFormatter,
            IStateProvinceService stateProvinceService,
            IWebHelper webHelper,
            IOrderService orderService,
            ICustomNumberFormatterService customNumberFormatterService,
            IProductAttributeFormatterService productAttributeFormatterService,
            IShippingService shippingService,
            IPriceCalculationService priceCalculationService,
            IWorkContextService workContextService,
            IWorkflowMessageService workflowMessageService,
            IEncryptionService encryptionService,
            IPdfService pdfService,
            IVendorService vendorService,
            ICustomerActivityService customerActivityService,
            IProductAttributeParserService productAttributeParserService,
            IShipmentService shipmentService,
            ILoggerService loggerService,
            IDiscountService discountService)
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _customerService = customerService;
            _paymentService = paymentService;
            _addressService = addressService;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
            _checkoutAttributeFormatterService = checkoutAttributeFormatterService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _priceFormatter = priceFormatter;
            _stateProvinceService = stateProvinceService;
            _webHelper = webHelper;
            _orderService = orderService;
            _customNumberFormatterService = customNumberFormatterService;
            _productAttributeFormatterService = productAttributeFormatterService;
            _shippingService = shippingService;
            _priceCalculationService = priceCalculationService;
            _workContextService = workContextService;
            _workflowMessageService = workflowMessageService;
            _encryptionService = encryptionService;
            _pdfService = pdfService;
            _vendorService = vendorService;
            _customerActivityService = customerActivityService;
            _productAttributeParserService = productAttributeParserService;
            _loggerService = loggerService;
            _shipmentService = shipmentService;
            _discountService = discountService;
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// PlaceOrder container
        /// </summary>
        protected class PlaceOrderContainer
        {
            public PlaceOrderContainer()
            {
                Cart = new List<ShoppingCartItem>();
                AppliedDiscounts = new List<Discount>();
            }

            /// <summary>
            /// Customer
            /// </summary>
            public Customer Customer { get; set; }

            /// <summary>
            /// Customer language
            /// </summary>
            public Language CustomerLanguage { get; set; }

            /// <summary>
            /// Affiliate identifier
            /// </summary>
            public int AffiliateId { get; set; }

            /// <summary>
            /// TAx display type
            /// </summary>
            public int CustomerTaxDisplayType { get; set; }

            /// <summary>
            /// Selected currency
            /// </summary>
            public string CustomerCurrencyCode { get; set; }

            /// <summary>
            /// Customer currency rate
            /// </summary>
            public decimal CustomerCurrencyRate { get; set; }

            /// <summary>
            /// Billing address
            /// </summary>
            public Address BillingAddress { get; set; }

            /// <summary>
            /// Shipping address
            /// </summary>
            public Address ShippingAddress { get; set; }

            /// <summary>
            /// Shipping status
            /// </summary>
            public ShippingStatus ShippingStatus { get; set; }

            /// <summary>
            /// Selected shipping method
            /// </summary>
            public string ShippingMethodName { get; set; }

            /// <summary>
            /// Shipping rate computation method system name
            /// </summary>
            public string ShippingRateComputationMethodSystemName { get; set; }

            /// <summary>
            /// Is pickup in store selected?
            /// </summary>
            public bool PickupInStore { get; set; }

            /// <summary>
            /// Selected pickup address
            /// </summary>
            public Address PickupAddress { get; set; }

            /// <summary>
            /// Is recurring shopping cart
            /// </summary>
            public bool IsRecurringShoppingCart { get; set; }

            /// <summary>
            /// Initial order (used with recurring payments)
            /// </summary>
            public Order InitialOrder { get; set; }

            /// <summary>
            /// Checkout attributes
            /// </summary>
            public string CheckoutAttributeDescription { get; set; }

            /// <summary>
            /// Shopping cart
            /// </summary>
            public string CheckoutAttributesXml { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public IList<ShoppingCartItem> Cart { get; set; }

            /// <summary>
            /// Applied discounts
            /// </summary>
            public List<Discount> AppliedDiscounts { get; set; }

            /// <summary>
            /// Order subtotal (incl tax)
            /// </summary>
            public decimal OrderSubTotalInclTax { get; set; }

            /// <summary>
            /// Order subtotal (excl tax)
            /// </summary>
            public decimal OrderSubTotalExclTax { get; set; }

            /// <summary>
            /// Subtotal discount (incl tax)
            /// </summary>
            public decimal OrderSubTotalDiscountInclTax { get; set; }

            /// <summary>
            /// Subtotal discount (excl tax)
            /// </summary>
            public decimal OrderSubTotalDiscountExclTax { get; set; }

            /// <summary>
            /// Shipping (incl tax)
            /// </summary>
            public decimal OrderShippingTotalInclTax { get; set; }

            /// <summary>
            /// Shipping (excl tax)
            /// </summary>
            public decimal OrderShippingTotalExclTax { get; set; }

            /// <summary>
            /// Payment additional fee (incl tax)
            /// </summary>
            public decimal PaymentAdditionalFeeInclTax { get; set; }

            /// <summary>
            /// Payment additional fee (excl tax)
            /// </summary>
            public decimal PaymentAdditionalFeeExclTax { get; set; }

            /// <summary>
            /// Tax
            /// </summary>
            public decimal OrderTaxTotal { get; set; }

            /// <summary>
            /// VAT number
            /// </summary>
            public string VatNumber { get; set; }

            /// <summary>
            /// Tax rates
            /// </summary>
            public string TaxRates { get; set; }

            /// <summary>
            /// Order total discount amount
            /// </summary>
            public decimal OrderDiscountAmount { get; set; }

            /// <summary>
            /// Redeemed reward points
            /// </summary>
            public int RedeemedRewardPoints { get; set; }

            /// <summary>
            /// Redeemed reward points amount
            /// </summary>
            public decimal RedeemedRewardPointsAmount { get; set; }

            /// <summary>
            /// Order total
            /// </summary>
            public decimal OrderTotal { get; set; }
        }

        #endregion

        #region Utilities

        protected virtual void ProcessOrderPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            ////raise event
            //_eventPublisher.Publish(new OrderPaidEvent(order));

            if (order.OrderTotal != decimal.Zero)
            {
                var orderPaidAttachmentFilePath = InovatiqaDefaults.AttachPdfInvoiceToOrderPaidEmail ?
                    _pdfService.PrintOrderToPdf(order) : null;
                var orderPaidAttachmentFileName = InovatiqaDefaults.AttachPdfInvoiceToOrderPaidEmail ?
                    "order.pdf" : null;
                var orderPaidCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderPaidCustomerNotification(order, order.CustomerLanguageId,
                    orderPaidAttachmentFilePath, orderPaidAttachmentFileName);

                if (orderPaidCustomerNotificationQueuedEmailIds.Any())
                    AddOrderNote(order, $"\"Order paid\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPaidCustomerNotificationQueuedEmailIds)}.");

                var orderPaidStoreOwnerNotificationQueuedEmailIds = _workflowMessageService.SendOrderPaidStoreOwnerNotification(order, InovatiqaDefaults.LanguageId);
                if (orderPaidStoreOwnerNotificationQueuedEmailIds.Any())
                    AddOrderNote(order, $"\"Order paid\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPaidStoreOwnerNotificationQueuedEmailIds)}.");

                var vendors = GetVendorsInOrder(order);
                foreach (var vendor in vendors)
                {
                    var orderPaidVendorNotificationQueuedEmailIds = _workflowMessageService.SendOrderPaidVendorNotification(order, vendor, InovatiqaDefaults.LanguageId);

                    if (orderPaidVendorNotificationQueuedEmailIds.Any())
                        AddOrderNote(order, $"\"Order paid\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPaidVendorNotificationQueuedEmailIds)}.");
                }
            }

            ProcessCustomerRolesWithPurchasedProductSpecified(order, true);
        }

        protected virtual void ProcessCustomerRolesWithPurchasedProductSpecified(Order order, bool add)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var purchasedProductIds = new List<int>();
            foreach (var orderItem in _orderService.GetOrderItems(order.Id))
            {
                purchasedProductIds.Add(orderItem.ProductId);

                var attributeValues = _productAttributeParserService.ParseProductAttributeValues(orderItem.AttributesXml);
                foreach (var attributeValue in attributeValues)
                {
                    if (attributeValue.AttributeValueTypeId == InovatiqaDefaults.AssociatedToProduct)
                    {
                        purchasedProductIds.Add(attributeValue.AssociatedProductId);
                    }
                }
            }

            var customerRoles = _customerService
                .GetAllCustomerRoles(true)
                .Where(cr => purchasedProductIds.Contains(cr.PurchasedWithProductId))
                .ToList();

            if (!customerRoles.Any())
                return;

            var customer = _customerService.GetCustomerById(order.CustomerId);

            foreach (var customerRole in customerRoles)
            {
                if (!_customerService.IsInCustomerRole(customer, customerRole.SystemName))
                {
                    //not in the list yet
                    if (add)
                    {
                        //add
                        _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                    }
                }
                else
                {
                    //already in the list
                    if (!add)
                    {
                        //remove
                        _customerService.RemoveCustomerRoleMapping(customer, customerRole);
                    }
                }
            }

            _customerService.UpdateCustomer(customer);
        }

        protected virtual void SetOrderStatus(Order order, OrderStatus os, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var prevOrderStatus = order.OrderStatusId;
            if (prevOrderStatus == (int)os)
                return;

            order.OrderStatusId = (int)os;
            _orderService.UpdateOrder(order);

            AddOrderNote(order, $"Order status has been changed to {os}");

            if (prevOrderStatus != (int)OrderStatus.Complete &&
                os == OrderStatus.Complete
                && notifyCustomer)
            {
                var orderCompletedAttachmentFilePath = InovatiqaDefaults.AttachPdfInvoiceToOrderCompletedEmail ?
                    _pdfService.PrintOrderToPdf(order) : null;
                var orderCompletedAttachmentFileName = InovatiqaDefaults.AttachPdfInvoiceToOrderCompletedEmail ?
                    "order.pdf" : null;
                var orderCompletedCustomerNotificationQueuedEmailIds = _workflowMessageService
                    .SendOrderCompletedCustomerNotification(order, order.CustomerLanguageId, orderCompletedAttachmentFilePath,
                    orderCompletedAttachmentFileName);
                if (orderCompletedCustomerNotificationQueuedEmailIds.Any())
                    AddOrderNote(order, $"\"Order completed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderCompletedCustomerNotificationQueuedEmailIds)}.");
            }

            if (prevOrderStatus != (int)OrderStatus.Cancelled &&
                os == OrderStatus.Cancelled
                && notifyCustomer)
            {
                var orderCancelledCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderCancelledCustomerNotification(order, order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailIds.Any())
                    AddOrderNote(order, $"\"Order cancelled\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderCancelledCustomerNotificationQueuedEmailIds)}.");
            }
        }

        protected virtual IList<Vendor> GetVendorsInOrder(Order order)
        {
            var pIds = _orderService.GetOrderItems(order.Id).Select(x => x.ProductId).ToArray();

            return _vendorService.GetVendorsByProductIds(pIds);
        }

        protected virtual void AddOrderNote(Order order, string note, int orderId = 0)
        {
            if (order == null)
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = orderId,
                    Note = note,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }
            else if (order != null)
            {
                _orderService.InsertOrderNote(new OrderNote
                {
                    OrderId = order.Id,
                    Note = note,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }
        }

        protected virtual void SendNotificationsAndSaveNotes(Order order)
        {
            AddOrderNote(order, _workContextService.OriginalCustomerIfImpersonated != null
                ? $"Order placed by a store owner ('{_workContextService.OriginalCustomerIfImpersonated.Email}'. ID = {_workContextService.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
                : "Order placed");

            var orderPlacedStoreOwnerNotificationQueuedEmailIds = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, InovatiqaDefaults.LanguageId);
            if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
                AddOrderNote(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.");

            var orderPlacedAttachmentFilePath = InovatiqaDefaults.AttachPdfInvoiceToOrderPlacedEmail ?
                _pdfService.PrintOrderToPdf(order) : null;
            var orderPlacedAttachmentFileName = InovatiqaDefaults.AttachPdfInvoiceToOrderPlacedEmail ?
                "order.pdf" : null;
            var orderPlacedCustomerNotificationQueuedEmailIds = _workflowMessageService
                .SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
            if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
                AddOrderNote(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}.");

            var vendors = GetVendorsInOrder(order);
            foreach (var vendor in vendors)
            {
                var orderPlacedVendorNotificationQueuedEmailIds = _workflowMessageService.SendOrderPlacedVendorNotification(order, vendor, InovatiqaDefaults.LanguageId);
                if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                    AddOrderNote(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
            }
        }

        protected virtual void MoveShoppingCartItemsToOrderItems(PlaceOrderContainer details, Order order)
        {
            foreach (var sc in details.Cart)
            {
                var product = _productService.GetProductById(sc.ProductId);

                var scUnitPrice = _shoppingCartService.GetUnitPrice(sc);
                var scSubTotal = _shoppingCartService.GetSubTotal(sc, true, out var discountAmount,
                    out var scDiscounts, out _);
                var scUnitPriceInclTax = scUnitPrice;
                var scSubTotalInclTax = scSubTotal;

                var attributeDescription =
                    _productAttributeFormatterService.FormatAttributes(product, sc.AttributesXml, details.Customer);

                var itemWeight = _shippingService.GetShoppingCartItemWeight(sc);

                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    UnitPriceInclTax = scUnitPriceInclTax,
                    UnitPriceExclTax = scUnitPriceInclTax,
                    PriceInclTax = scSubTotalInclTax,
                    PriceExclTax = scSubTotalInclTax,
                    OriginalProductCost = _priceCalculationService.GetProductCost(product, sc.AttributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = sc.AttributesXml,
                    Quantity = sc.Quantity,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = sc.RentalStartDateUtc,
                    RentalEndDateUtc = sc.RentalEndDateUtc,
                    ReorderDateUtc = sc.Reordered ? DateTime.Now : (DateTime?)null
                };

                _orderService.InsertOrderItem(orderItem);

                _productService.AdjustInventory(product, -sc.Quantity, sc.AttributesXml,
                    string.Format("The stock quantity has been reduced by placing the order #{0}", order.Id));
            }
            details.Cart.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci, false));
        }

        protected virtual Order SaveOrderDetails(ProcessPaymentRequest processPaymentRequest,
            ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
        {
            var order = new Order
            {
                StoreId = processPaymentRequest.StoreId,
                OrderGuid = processPaymentRequest.OrderGuid,
                CustomerId = details.Customer.Id,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayTypeId = details.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = details.OrderSubTotalInclTax,
                OrderSubtotalExclTax = details.OrderSubTotalExclTax,
                OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
                OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
                OrderShippingInclTax = details.OrderShippingTotalInclTax,
                OrderShippingExclTax = details.OrderShippingTotalExclTax,
                PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                TaxRates = details.TaxRates,
                OrderTax = details.OrderTaxTotal,
                OrderTotal = details.OrderTotal,
                RefundedAmount = decimal.Zero,
                OrderDiscount = details.OrderDiscountAmount,
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributesXml = details.CheckoutAttributesXml,
                CustomerCurrencyCode = details.CustomerCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                AffiliateId = details.AffiliateId,
                OrderStatusId = (int)OrderStatus.Pending,
                AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                PaymentStatusId = (int)processPaymentResult.NewPaymentStatus,
                PaidDateUtc = null,
                PickupInStore = details.PickupInStore,
                ShippingStatusId = (int)details.ShippingStatus,
                ShippingMethod = details.ShippingMethodName,
                ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
                VatNumber = details.VatNumber,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty
            };

            if (details.BillingAddress is null)
                throw new InovatiqaException("Billing address is not provided");

            _addressService.InsertAddress(details.BillingAddress);
            order.BillingAddressId = details.BillingAddress.Id;

            if (details.PickupAddress != null)
            {
                _addressService.InsertAddress(details.PickupAddress);
                order.PickupAddressId = details.PickupAddress.Id;
            }

            if (details.ShippingAddress != null)
            {
                _addressService.InsertAddress(details.ShippingAddress);
                order.ShippingAddressId = details.ShippingAddress.Id;
            }

            _orderService.InsertOrder(order);

            order.CustomOrderNumber = _customNumberFormatterService.GenerateOrderCustomNumber(order);
            _orderService.UpdateOrder(order);

            _customerService.UpdateCustomer(details.Customer);

            return order;
        }

        protected virtual ProcessPaymentResult GetProcessPaymentResult(ProcessPaymentRequest processPaymentRequest, PlaceOrderContainer details)
        {
            ProcessPaymentResult processPaymentResult;
            var skipPaymentWorkflow = details.OrderTotal == decimal.Zero;
            if (!skipPaymentWorkflow)
            {
                //var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

                if (details.IsRecurringShoppingCart)
                {
                    switch (_paymentService.GetRecurringPaymentType(processPaymentRequest.PaymentMethodSystemName))
                    {
                        case InovatiqaDefaults.NotSupported:
                            throw new InovatiqaException("Recurring payments are not supported by selected payment method");
                        case InovatiqaDefaults.Manual:
                        case InovatiqaDefaults.Automatic:
                            processPaymentResult = _paymentService.ProcessRecurringPayment(processPaymentRequest);
                            break;
                        default:
                            throw new InovatiqaException("Not supported recurring payment type");
                    }
                }
                else
                    processPaymentResult = _paymentService.ProcessPayment(processPaymentRequest);
            }
            else
                processPaymentResult = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Paid };
            return processPaymentResult;
        }

        protected virtual PlaceOrderContainer PreparePlaceOrderDetails(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainer
            {
                Customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId)
            };
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            if (_customerService.IsGuest(details.Customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                throw new InovatiqaException("Anonymous checkout is not allowed");

            var currencyTmp = InovatiqaDefaults.CurrencyCode;
            var customerCurrency = currencyTmp;
            var primaryStoreCurrency = customerCurrency;
            details.CustomerCurrencyCode = primaryStoreCurrency;
            details.CustomerCurrencyRate = InovatiqaDefaults.CurrencyRate;

            Language lang = new Language();
            lang.DefaultCurrencyId = InovatiqaDefaults.LanguageDefaultCurrencyId;
            lang.DisplayOrder = InovatiqaDefaults.DisplayOrder;
            lang.FlagImageFileName = InovatiqaDefaults.FlagImageFileName;
            lang.LimitedToStores = InovatiqaDefaults.LimitedToStores;
            lang.Name = InovatiqaDefaults.LanguageName;
            lang.Rtl = InovatiqaDefaults.Rtl;
            lang.UniqueSeoCode = InovatiqaDefaults.UniqueSeoCode;

            details.CustomerLanguage = lang;

            details.CustomerLanguage.LanguageCulture = InovatiqaDefaults.LanguageCulture;

            if (details.Customer.BillingAddressId is null)
                throw new InovatiqaException("Billing address is not provided");

            var billingAddress = _customerService.GetCustomerBillingAddress(details.Customer);

            if (!CommonHelper.IsValidEmail(billingAddress?.Email))
                throw new InovatiqaException("Email is not valid");

            details.BillingAddress = _addressService.CloneAddress(billingAddress, details.Customer, true);

            if (_countryService.GetCountryByAddress(details.BillingAddress) is Country billingCountry && !billingCountry.AllowsBilling)
                throw new InovatiqaException($"Country '{billingCountry.Name}' is not allowed for billing");

            details.CheckoutAttributesXml = _genericAttributeService.GetAttribute<string>(details.Customer, InovatiqaDefaults.CheckoutAttributes, processPaymentRequest.StoreId);
            details.CheckoutAttributeDescription = _checkoutAttributeFormatterService.FormatAttributes(details.CheckoutAttributesXml, details.Customer);

            details.Cart = _shoppingCartService.GetShoppingCart(details.Customer, (int)ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId);

            if (!details.Cart.Any())
                throw new InovatiqaException("Cart is empty");

            var warnings = _shoppingCartService.GetShoppingCartWarnings(details.Cart, details.CheckoutAttributesXml, true);
            if (warnings.Any())
                throw new InovatiqaException(warnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));

            foreach (var sci in details.Cart)
            {
                var product = _productService.GetProductById(sci.ProductId);

                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(details.Customer,
                    sci.ShoppingCartTypeId, product, processPaymentRequest.StoreId, sci.AttributesXml,
                    sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false, sci.Id);
                if (sciWarnings.Any())
                    throw new InovatiqaException(sciWarnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));
            }

            if (!ValidateMinOrderSubtotalAmount(details.Cart))
            {
                var minOrderSubtotalAmount = InovatiqaDefaults.MinOrderSubtotalAmount;
                throw new InovatiqaException(string.Format("Minimum order sub-total amount is {0}",
                    _priceFormatter.FormatPrice(minOrderSubtotalAmount)));
            }

            if (!ValidateMinOrderTotalAmount(details.Cart))
            {
                var minOrderTotalAmount = InovatiqaDefaults.MinOrderTotalAmount;
                throw new InovatiqaException(string.Format("Minimum order total amount is {0}",
                    _priceFormatter.FormatPrice(minOrderTotalAmount)));
            }

            _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, true, out var orderSubTotalDiscountAmount, out var orderSubTotalAppliedDiscounts, out var subTotalWithoutDiscountBase, out var _);
            details.OrderSubTotalInclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

            _orderTotalCalculationService.GetShoppingCartSubTotal(details.Cart, false, out orderSubTotalDiscountAmount,
                out orderSubTotalAppliedDiscounts, out subTotalWithoutDiscountBase, out _);
            details.OrderSubTotalExclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;

            if (_shoppingCartService.ShoppingCartRequiresShipping(details.Cart))
            {
                var pickupPoint = _genericAttributeService.GetAttribute<PickupPoint>(details.Customer,
                    InovatiqaDefaults.SelectedPickupPointAttribute, processPaymentRequest.StoreId);
                if (InovatiqaDefaults.AllowPickupInStore && pickupPoint != null)
                {
                    var country = _countryService.GetCountryByTwoLetterIsoCode(pickupPoint.CountryCode);
                    var state = _stateProvinceService.GetStateProvinceByAbbreviation(pickupPoint.StateAbbreviation, country?.Id);

                    details.PickupInStore = true;
                    details.PickupAddress = new Address
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        County = pickupPoint.County,
                        CountryId = country?.Id,
                        StateProvinceId = state?.Id,
                        ZipPostalCode = pickupPoint.ZipPostalCode,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                }
                else
                {
                    if (details.Customer.ShippingAddressId == null)
                        throw new InovatiqaException("Shipping address is not provided");

                    var shippingAddress = _customerService.GetCustomerShippingAddress(details.Customer);

                    if (!CommonHelper.IsValidEmail(shippingAddress?.Email))
                        throw new InovatiqaException("Email is not valid");

                    details.ShippingAddress = _addressService.CloneAddress(shippingAddress, details.Customer, false);

                    if (_countryService.GetCountryByAddress(details.ShippingAddress) is Country shippingCountry && !shippingCountry.AllowsShipping)
                        throw new InovatiqaException($"Country '{shippingCountry.Name}' is not allowed for shipping");
                }

                var shippingOption = _genericAttributeService.GetAttribute<ShippingOption>(details.Customer,
                    InovatiqaDefaults.SelectedShippingOptionAttribute, details.Customer.Id, processPaymentRequest.StoreId);
                if (shippingOption != null)
                {
                    details.ShippingMethodName = shippingOption.Name;
                    details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                }

                details.ShippingStatus = ShippingStatus.NotYetShipped;
            }
            else
                details.ShippingStatus = ShippingStatus.ShippingNotRequired;

            var orderShippingTotalInclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, true, out var _, out var shippingTotalDiscounts);
            var orderShippingTotalExclTax = _orderTotalCalculationService.GetShoppingCartShippingTotal(details.Cart, false);
            if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                throw new InovatiqaException("Shipping total couldn't be calculated");

            details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
            details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

            var paymentAdditionalFee = _paymentService.GetAdditionalHandlingFee(details.Cart, processPaymentRequest.PaymentMethodSystemName);
            details.PaymentAdditionalFeeInclTax = paymentAdditionalFee;
            details.PaymentAdditionalFeeExclTax = paymentAdditionalFee;

            details.OrderTaxTotal = _orderTotalCalculationService.GetTaxTotal(details.Cart, out var taxRatesDictionary);


            var orderTotal = _orderTotalCalculationService.GetShoppingCartTotal(details.Cart, out var orderDiscountAmount, out var orderAppliedDiscounts, out var appliedGiftCards, out var redeemedRewardPoints);
            if (!orderTotal.HasValue)
                throw new InovatiqaException("Order total couldn't be calculated");

            details.OrderDiscountAmount = orderDiscountAmount;
            details.OrderTotal = orderTotal.Value;


            processPaymentRequest.OrderTotal = details.OrderTotal;

            return details;
        }

        protected virtual void RemoveSuspendedShoppingCartItems(PlaceOrderContainer details)
        {
            foreach (var sci in details.Cart)
            {
                if (details.Cart.Count > 0)
                {
                    if (sci.ParentSuspendedItemId != null)
                    {
                        var suspendedCart = _shoppingCartService.GetSuspendedShoppingCartById(details.Customer, int.Parse(sci.ParentSuspendedItemId.ToString()));
                        _shoppingCartService.DeleteSuspendedShoppingCartItemById(int.Parse(sci.ParentSuspendedItemId.ToString()));
                        if (suspendedCart != null)
                        {
                            if (suspendedCart.Lines > 0)
                                suspendedCart.Lines--;
                            _shoppingCartService.UpdateSuspendedShoppingCart(suspendedCart);
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        public virtual IList<string> Capture(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanCapture(order))
                throw new InovatiqaException("Cannot do capture for order.");

            var request = new CapturePaymentRequest();
            CapturePaymentResult result = null;
            try
            {
                request.Order = order;
                result = _paymentService.Capture(request);

                if (result.Success)
                {
                    var paidDate = order.PaidDateUtc;
                    if (result.NewPaymentStatus == PaymentStatus.Paid)
                        paidDate = DateTime.UtcNow;

                    order.CaptureTransactionId = result.CaptureTransactionId;
                    order.CaptureTransactionResult = result.CaptureTransactionResult;
                    order.PaymentStatusId = (int)result.NewPaymentStatus;
                    order.PaidDateUtc = paidDate;
                    _orderService.UpdateOrder(order);

                    AddOrderNote(order, "Order has been captured");

                    CheckOrderStatus(order);

                    if (order.PaymentStatusId == (int)PaymentStatus.Paid)
                    {
                        ProcessOrderPaid(order);
                    }
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CapturePaymentResult();
                result.AddError($"Error: {exc.Message}. Full exception: {exc}");
            }

            //process errors
            var error = string.Empty;
            for (var i = 0; i < result.Errors.Count; i++)
            {
                error += $"Error {i}: {result.Errors[i]}";
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }

            if (string.IsNullOrEmpty(error))
                return result.Errors;

            //add a note
            AddOrderNote(order, $"Unable to capture order. {error}");

            //log it
            var logError = $"Error capturing order #{order.Id}. Error: {error}";
            _loggerService.InsertLog((int)LogLevel.Error, logError, logError);
            return result.Errors;
        }

        public virtual void UpdateOrderTotals(UpdateOrderParameters updateOrderParameters)
        {
            var updatedOrder = updateOrderParameters.UpdatedOrder;
            var updatedOrderItem = updateOrderParameters.UpdatedOrderItem;

            var (restoredCart, updatedShoppingCartItem) = restoreShoppingCart(updatedOrder, updatedOrderItem.Id);

            var itemDeleted = updatedShoppingCartItem is null;

            updateOrderParameters.Warnings.AddRange(_shoppingCartService.GetShoppingCartWarnings(restoredCart, string.Empty, false));

            var customer = _customerService.GetCustomerById(updatedOrder.CustomerId);

            if (!itemDeleted)
            {
                var product = _productService.GetProductById(updatedShoppingCartItem.ProductId);

                updateOrderParameters.Warnings.AddRange(_shoppingCartService.GetShoppingCartItemWarnings(customer, updatedShoppingCartItem.ShoppingCartTypeId,
                    product, updatedOrder.StoreId, updatedShoppingCartItem.AttributesXml, updatedShoppingCartItem.CustomerEnteredPrice,
                    updatedShoppingCartItem.RentalStartDateUtc, updatedShoppingCartItem.RentalEndDateUtc, updatedShoppingCartItem.Quantity, false, updatedShoppingCartItem.Id));

                updatedOrderItem.ItemWeight = _shippingService.GetShoppingCartItemWeight(updatedShoppingCartItem);
                updatedOrderItem.OriginalProductCost = _priceCalculationService.GetProductCost(product, updatedShoppingCartItem.AttributesXml);
                updatedOrderItem.AttributeDescription = _productAttributeFormatterService.FormatAttributes(product,
                    updatedShoppingCartItem.AttributesXml, customer);

                //gift cards
                //AddGiftCards(product, updatedShoppingCartItem.AttributesXml, updatedShoppingCartItem.Quantity, updatedOrderItem, updatedOrderItem.UnitPriceExclTax);
            }

            _orderTotalCalculationService.UpdateOrderTotals(updateOrderParameters, restoredCart);

            if (updateOrderParameters.PickupPoint != null)
            {
                updatedOrder.PickupInStore = true;

                var pickupAddress = new Address
                {
                    Address1 = updateOrderParameters.PickupPoint.Address,
                    City = updateOrderParameters.PickupPoint.City,
                    County = updateOrderParameters.PickupPoint.County,
                    CountryId = _countryService.GetCountryByTwoLetterIsoCode(updateOrderParameters.PickupPoint.CountryCode)?.Id,
                    ZipPostalCode = updateOrderParameters.PickupPoint.ZipPostalCode,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _addressService.InsertAddress(pickupAddress);

                updatedOrder.PickupAddressId = pickupAddress.Id;
                updatedOrder.ShippingMethod = string.Format("Pickup at {0}", updateOrderParameters.PickupPoint.Name);
                updatedOrder.ShippingRateComputationMethodSystemName = updateOrderParameters.PickupPoint.ProviderSystemName;
            }

            _orderService.UpdateOrder(updatedOrder);

            var discountUsageHistoryForOrder = _discountService.GetAllDiscountUsageHistory(null, customer.Id, updatedOrder.Id);
            foreach (var discount in updateOrderParameters.AppliedDiscounts)
            {
                if (discountUsageHistoryForOrder.Any(history => history.DiscountId == discount.Id))
                    continue;

                var d = _discountService.GetDiscountById(discount.Id);
                if (d != null)
                {
                    _discountService.InsertDiscountUsageHistory(new DiscountUsageHistory
                    {
                        DiscountId = d.Id,
                        OrderId = updatedOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
            }

            CheckOrderStatus(updatedOrder);

            (List<ShoppingCartItem> restoredCart, ShoppingCartItem updatedShoppingCartItem) restoreShoppingCart(Order order, int updatedOrderItemId)
            {
                if (order is null)
                    throw new ArgumentNullException(nameof(order));

                var cart = _orderService.GetOrderItems(order.Id).Select(item => new ShoppingCartItem
                {
                    Id = item.Id,
                    AttributesXml = item.AttributesXml,
                    CustomerId = order.CustomerId,
                    ProductId = item.ProductId,
                    Quantity = item.Id == updatedOrderItemId ? updateOrderParameters.Quantity : item.Quantity,
                    RentalEndDateUtc = item.RentalEndDateUtc,
                    RentalStartDateUtc = item.RentalStartDateUtc,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    StoreId = order.StoreId
                }).ToList();

                //get shopping cart item which has been updated
                var cartItem = cart.FirstOrDefault(shoppingCartItem => shoppingCartItem.Id == updatedOrderItemId);

                return (cart, cartItem);
            }
        }

        public virtual void PartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanPartiallyRefundOffline(order, amountToRefund))
                throw new InovatiqaException("You can't partially refund (offline) this order");

            var totalAmountRefunded = order.RefundedAmount + amountToRefund;


            order.RefundedAmount = totalAmountRefunded;

            order.PaymentStatusId = order.OrderTotal == totalAmountRefunded ? (int)PaymentStatus.Refunded : (int)PaymentStatus.PartiallyRefunded;
            _orderService.UpdateOrder(order);

            AddOrderNote(order, $"Order has been marked as partially refunded. Amount = {amountToRefund}");

            CheckOrderStatus(order);

            var orderRefundedStoreOwnerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, InovatiqaDefaults.LanguageId);
            if (orderRefundedStoreOwnerNotificationQueuedEmailIds.Any())
                AddOrderNote(order, $"\"Order refunded\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedStoreOwnerNotificationQueuedEmailIds)}.");

            var orderRefundedCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            if (orderRefundedCustomerNotificationQueuedEmailIds.Any())
                AddOrderNote(order, $"\"Order refunded\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedCustomerNotificationQueuedEmailIds)}.");

            //raise event       
            //_eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
        }

        public virtual IList<string> PartiallyRefund(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanPartiallyRefund(order, amountToRefund))
                throw new InovatiqaException("Cannot do partial refund for order.");

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            try
            {
                request.Order = order;
                request.AmountToRefund = amountToRefund;
                request.IsPartialRefund = true;

                result = _paymentService.Refund(request);

                if (result.Success)
                {
                    var totalAmountRefunded = order.RefundedAmount + amountToRefund;

                    order.RefundedAmount = totalAmountRefunded;

                    order.PaymentStatusId = order.OrderTotal == totalAmountRefunded && result.NewPaymentStatus == PaymentStatus.PartiallyRefunded ? (int)PaymentStatus.Refunded : (int)result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);

                    AddOrderNote(order, $"Order has been partially refunded. Amount = {amountToRefund}");

                    CheckOrderStatus(order);

                    var orderRefundedStoreOwnerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, InovatiqaDefaults.LanguageId);
                    if (orderRefundedStoreOwnerNotificationQueuedEmailIds.Any())
                        AddOrderNote(order, $"\"Order refunded\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedStoreOwnerNotificationQueuedEmailIds)}.");

                    var orderRefundedCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
                    if (orderRefundedCustomerNotificationQueuedEmailIds.Any())
                        AddOrderNote(order, $"\"Order refunded\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedCustomerNotificationQueuedEmailIds)}.");

                    //raise event       
                    //_eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError($"Error: {exc.Message}. Full exception: {exc}");
            }

            var error = string.Empty;
            for (var i = 0; i < result.Errors.Count; i++)
            {
                error += $"Error {i}: {result.Errors[i]}";
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }

            if (string.IsNullOrEmpty(error))
                return result.Errors;

            AddOrderNote(order, $"Unable to partially refund order. {error}");

            var logError = $"Error refunding order #{order.Id}. Error: {error}";
            _loggerService.InsertLog((int)LogLevel.Error, logError, logError);
            return result.Errors;
        }

        public virtual void RefundOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanRefundOffline(order))
                throw new InovatiqaException("You can't refund this order");

            var amountToRefund = order.OrderTotal;

            var totalAmountRefunded = order.RefundedAmount + amountToRefund;

            order.RefundedAmount = totalAmountRefunded;
            order.PaymentStatusId = (int)PaymentStatus.Refunded;
            _orderService.UpdateOrder(order);

            AddOrderNote(order, $"Order has been marked as refunded. Amount = {amountToRefund}");

            CheckOrderStatus(order);

            var orderRefundedStoreOwnerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, amountToRefund, InovatiqaDefaults.LanguageId);
            if (orderRefundedStoreOwnerNotificationQueuedEmailIds.Any())
                AddOrderNote(order, $"\"Order refunded\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedStoreOwnerNotificationQueuedEmailIds)}.");

            var orderRefundedCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedCustomerNotification(order, amountToRefund, order.CustomerLanguageId);
            if (orderRefundedCustomerNotificationQueuedEmailIds.Any())
                AddOrderNote(order, $"\"Order refunded\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedCustomerNotificationQueuedEmailIds)}.");

            //raise event       
            //_eventPublisher.Publish(new OrderRefundedEvent(order, amountToRefund));
        }

        public virtual IList<string> Refund(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanRefund(order))
                throw new InovatiqaException("Cannot do refund for order.");

            var request = new RefundPaymentRequest();
            RefundPaymentResult result = null;
            try
            {
                request.Order = order;
                request.AmountToRefund = order.OrderTotal;
                request.IsPartialRefund = false;
                result = _paymentService.Refund(request);
                if (result.Success)
                {
                    var totalAmountRefunded = order.RefundedAmount + request.AmountToRefund;

                    order.RefundedAmount = totalAmountRefunded;
                    order.PaymentStatusId = (int)result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);

                    AddOrderNote(order, $"Order has been refunded. Amount = {request.AmountToRefund}");

                    CheckOrderStatus(order);

                    var orderRefundedStoreOwnerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, request.AmountToRefund, InovatiqaDefaults.LanguageId);
                    if (orderRefundedStoreOwnerNotificationQueuedEmailIds.Any())
                        AddOrderNote(order, $"\"Order refunded\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedStoreOwnerNotificationQueuedEmailIds)}.");

                    var orderRefundedCustomerNotificationQueuedEmailIds = _workflowMessageService.SendOrderRefundedCustomerNotification(order, request.AmountToRefund, order.CustomerLanguageId);
                    if (orderRefundedCustomerNotificationQueuedEmailIds.Any())
                        AddOrderNote(order, $"\"Order refunded\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderRefundedCustomerNotificationQueuedEmailIds)}.");

                    //raise event       
                    //_eventPublisher.Publish(new OrderRefundedEvent(order, request.AmountToRefund));
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new RefundPaymentResult();
                result.AddError($"Error: {exc.Message}. Full exception: {exc}");
            }

            //process errors
            var error = string.Empty;
            for (var i = 0; i < result.Errors.Count; i++)
            {
                error += $"Error {i}: {result.Errors[i]}";
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }

            if (string.IsNullOrEmpty(error))
                return result.Errors;

            AddOrderNote(order, $"Unable to refund order. {error}");

            var logError = $"Error refunding order #{order.Id}. Error: {error}";
            _loggerService.InsertLog((int)LogLevel.Error, logError, logError);
            return result.Errors;
        }

        public virtual void MarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanMarkOrderAsPaid(order))
                throw new InovatiqaException("You can't mark this order as paid");

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;
            _orderService.UpdateOrder(order);

            AddOrderNote(order, "Order has been marked as paid");

            CheckOrderStatus(order);

            if (order.PaymentStatusId == (int)PaymentStatus.Paid)
            {
                ProcessOrderPaid(order);
            }
        }

        public virtual bool IsReturnRequestAllowed(Order order)
        {
            if (!InovatiqaDefaults.ReturnRequestsEnabled)
                return false;

            if (order == null || order.Deleted)
                return false;

            if (order.OrderStatusId != (int)OrderStatus.Complete)
                return false;

            if (InovatiqaDefaults.NumberOfDaysReturnRequestAvailable <= 0)
                return _orderService.GetOrderItems(order.Id, isNotReturnable: false).Any();

            var daysPassed = (DateTime.UtcNow - order.CreatedOnUtc).TotalDays;

            if (daysPassed >= InovatiqaDefaults.NumberOfDaysReturnRequestAvailable)
                return false;

            return _orderService.GetOrderItems(order.Id, isNotReturnable: false).Any();
        }

        public virtual void CheckOrderStatus(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.PaymentStatusId == (int)PaymentStatus.Paid && !order.PaidDateUtc.HasValue)
            {
                order.PaidDateUtc = DateTime.UtcNow;
                _orderService.UpdateOrder(order);
            }

            switch (order.OrderStatusId)
            {
                case (int)OrderStatus.Pending:
                    if (order.PaymentStatusId == (int)PaymentStatus.Authorized || order.PaymentMethodSystemName == InovatiqaDefaults.PurchaseOrderPaymentName)
                    {
                        SetOrderStatus(order, OrderStatus.Submitted, false);
                    }
                    break;
                case (int)OrderStatus.Submitted:
                    if (order.PaymentStatusId == (int)PaymentStatus.Paid)
                    {
                        SetOrderStatus(order, OrderStatus.Processing, false);
                    }
                    break;
                case (int)OrderStatus.Processing:
                    if (order.ShippingStatusId == (int)ShippingStatus.Shipped ||
                        order.ShippingStatusId == (int)ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Complete, false);
                    }
                    if (order.ShippingStatusId == (int)ShippingStatus.PartiallyShipped)
                    {
                        SetOrderStatus(order, OrderStatus.Backorder, false);
                    }
                    break;
                case (int)OrderStatus.Backorder:
                    if (order.ShippingStatusId == (int)ShippingStatus.Shipped ||
                        order.ShippingStatusId == (int)ShippingStatus.Delivered)
                    {
                        SetOrderStatus(order, OrderStatus.Complete, false);
                    }
                    if (order.ShippingStatusId == (int)ShippingStatus.PartiallyShipped)
                    {
                        SetOrderStatus(order, OrderStatus.Backorder, false);
                    }
                    break;
                case (int)OrderStatus.Cancelled:
                    return;
            }

            if (order.PaymentStatusId != (int)PaymentStatus.Paid)
                return;

            bool completed;

            if (order.ShippingStatusId == (int)ShippingStatus.ShippingNotRequired)
            {
                completed = true;
            }
            else
            {
                if (InovatiqaDefaults.CompleteOrderWhenDelivered)
                {
                    completed = order.ShippingStatusId == (int)ShippingStatus.Delivered;
                }
                else
                {
                    completed = order.ShippingStatusId == (int)ShippingStatus.Shipped ||
                                order.ShippingStatusId == (int)ShippingStatus.Delivered;
                }
            }

            if (completed)
            {
                SetOrderStatus(order, OrderStatus.Complete, true);
            }
        }

        public virtual bool ValidateMinOrderSubtotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (!cart.Any() || InovatiqaDefaults.MinOrderSubtotalAmount <= decimal.Zero)
                return true;

            _orderTotalCalculationService.GetShoppingCartSubTotal(cart, InovatiqaDefaults.MinOrderSubtotalAmountIncludingTax, out var _, out var _, out var subTotalWithoutDiscountBase, out var _);

            if (subTotalWithoutDiscountBase < InovatiqaDefaults.MinOrderSubtotalAmount)
                return false;

            return true;
        }

        public virtual bool IsPaymentWorkflowRequired(IList<ShoppingCartItem> cart, bool? useRewardPoints = null)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var result = true;
            var shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart, useRewardPoints: useRewardPoints);
            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value == decimal.Zero)
                result = false;
            return result;
        }

        public virtual bool ValidateMinOrderTotalAmount(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (!cart.Any() || InovatiqaDefaults.MinOrderTotalAmount <= decimal.Zero)
                return true;

            var shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart);

            if (shoppingCartTotalBase.HasValue && shoppingCartTotalBase.Value < InovatiqaDefaults.MinOrderTotalAmount)
                return false;

            return true;
        }

        public virtual PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentNullException(nameof(processPaymentRequest));

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                    throw new Exception("Order GUID is not generated");

                var details = PreparePlaceOrderDetails(processPaymentRequest);

                var processPaymentResult = GetProcessPaymentResult(processPaymentRequest, details);

                if (processPaymentResult == null)
                    throw new InovatiqaException("processPaymentResult is not available");

                if (processPaymentResult.Success)
                {
                    var order = SaveOrderDetails(processPaymentRequest, processPaymentResult, details);
                    result.PlacedOrder = order;

                    MoveShoppingCartItemsToOrderItems(details, order);

                    RemoveSuspendedShoppingCartItems(details);

                    SendNotificationsAndSaveNotes(order);

                    _customerService.ResetCheckoutData(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                    _customerActivityService.InsertActivity("PublicStore.PlaceOrder",
                        string.Format("Placed a new order (ID = {0})", order.Id), order.Id, order.GetType().Name);

                    CheckOrderStatus(order);

                    ////raise event       
                    //_eventPublisher.Publish(new OrderPlacedEvent(order));

                    if (order.PaymentStatusId == (int)PaymentStatus.Paid)
                        ProcessOrderPaid(order);
                }
                else
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format("Payment error: {0}", paymentError));
            }
            catch (Exception exc)
            {
                _loggerService.Error(exc.Message, exc);
                result.AddError(exc.Message);
            }

            if (result.Success)
                return result;

            var logError = result.Errors.Aggregate("Error while placing order. ",
                (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            _loggerService.Error(logError, customer: customer);

            return result;
        }

        public virtual void ReOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var customer = _customerService.GetCustomerById(order.CustomerId);

            foreach (var orderItem in _orderService.GetOrderItems(order.Id))
            {
                var product = _productService.GetProductById(orderItem.ProductId);

                _shoppingCartService.AddToCart(customer, product,
                    (int)ShoppingCartType.ShoppingCart, order.StoreId,
                    orderItem.AttributesXml, orderItem.UnitPriceExclTax,
                    orderItem.RentalStartDateUtc, orderItem.RentalEndDateUtc,
                    orderItem.Quantity, false, 0, true);
            }

            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CheckoutAttributes, order.CheckoutAttributesXml, order.StoreId);
        }

        public virtual void ReOrderOrderedItem(Order order, int orderLineId, int qty, Microsoft.AspNetCore.Http.IFormCollection form)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var customer = _customerService.GetCustomerById(order.CustomerId);

            foreach (var orderItem in _orderService.GetOrderItems(order.Id))
            {
                if (orderItem.Id == orderLineId)
                {
                    var product = _productService.GetProductById(orderItem.ProductId);
                    var addToCartWarnings = new List<string>();
                    var attributes = "";
                    if (form.Where(a => a.Key.Contains("attr")).Select(x => x.Key).Count() > 0)
                        attributes = _productAttributeParserService.ParseProductAttributes(product, form, addToCartWarnings);
                    else
                        attributes = orderItem.AttributesXml;
                    _shoppingCartService.AddToCart(customer, product,
                        (int)ShoppingCartType.ShoppingCart, order.StoreId,
                        attributes, orderItem.UnitPriceExclTax,
                        orderItem.RentalStartDateUtc, orderItem.RentalEndDateUtc,
                        qty, false, 0, true);
                }
            }

            _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CheckoutAttributes, order.CheckoutAttributesXml, order.StoreId);
        }

        public virtual bool CanCancelOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderStatusId == (int)OrderStatus.Cancelled)
                return false;

            return true;
        }

        public virtual bool CanCapture(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderStatusId == (int)OrderStatus.Cancelled ||
                order.OrderStatusId == (int)OrderStatus.Processing)
                return false;

            if (order.PaymentStatusId == (int)PaymentStatus.Authorized &&
                _paymentService.SupportCapture(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        public virtual bool CanMarkOrderAsPaid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderStatusId == (int)OrderStatus.Cancelled)
                return false;

            if (order.PaymentStatusId == (int)PaymentStatus.Paid ||
                order.PaymentStatusId == (int)PaymentStatus.Refunded ||
                order.PaymentStatusId == (int)PaymentStatus.Voided)
                return false;

            return true;
        }

        public virtual bool CanRefund(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                return false;

            if (order.RefundedAmount > decimal.Zero)
                return false;

            //uncomment the lines below in order to disallow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatusId == (int)PaymentStatus.Paid &&
                _paymentService.SupportRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        public virtual bool CanRefundOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                return false;

            //refund cannot be made if previously a partial refund has been already done. only other partial refund can be made in this case
            if (order.RefundedAmount > decimal.Zero)
                return false;

            //uncomment the lines below in order to disallow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //     return false;

            if (order.PaymentStatusId == (int)PaymentStatus.Paid)
                return true;

            return false;
        }

        public virtual bool CanPartiallyRefund(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            var canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if ((order.PaymentStatusId == (int)PaymentStatus.Paid ||
                order.PaymentStatusId == (int)PaymentStatus.PartiallyRefunded) &&
                _paymentService.SupportPartiallyRefund(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        public virtual bool CanPartiallyRefundOffline(Order order, decimal amountToRefund)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            var canBeRefunded = order.OrderTotal - order.RefundedAmount;
            if (canBeRefunded <= decimal.Zero)
                return false;

            if (amountToRefund > canBeRefunded)
                return false;

            if (order.PaymentStatusId == (int)PaymentStatus.Paid ||
                order.PaymentStatusId == (int)PaymentStatus.PartiallyRefunded)
                return true;

            return false;
        }

        public virtual bool CanVoid(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatusId == (int)PaymentStatus.Authorized &&
                _paymentService.SupportVoid(order.PaymentMethodSystemName))
                return true;

            return false;
        }

        public virtual bool CanVoidOffline(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderTotal == decimal.Zero)
                return false;

            //uncomment the lines below in order to allow this operation for cancelled orders
            //if (order.OrderStatus == OrderStatus.Cancelled)
            //    return false;

            if (order.PaymentStatusId == (int)PaymentStatus.Authorized)
                return true;

            return false;
        }

        public virtual void CancelOrder(Order order, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanCancelOrder(order))
                throw new InovatiqaException("Cannot do cancel for order.");

            SetOrderStatus(order, OrderStatus.Cancelled, notifyCustomer);


            AddOrderNote(order, "Order has been cancelled");


            foreach (var shipment in _shipmentService.GetShipmentsByOrderId(order.Id))
            {
                foreach (var shipmentItem in _shipmentService.GetShipmentItemsByShipmentId(shipment.Id))
                {
                    var product = _orderService.GetProductByOrderItemId(shipmentItem.OrderItemId);

                    if (product is null)
                        continue;

                    _productService.ReverseBookedInventory(product, shipmentItem,
                        string.Format("The stock quantity has been increased by canceling the order #{0}", order.Id));
                }
            }

            foreach (var orderItem in _orderService.GetOrderItems(order.Id))
            {
                var product = _productService.GetProductById(orderItem.ProductId);

                _productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format("The stock quantity has been increased by canceling the order #{0}", order.Id));
            }

            //_eventPublisher.Publish(new OrderCancelledEvent(order));
        }

        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (order.OrderStatusId != (int)OrderStatus.Cancelled)
            {
                foreach (var shipment in _shipmentService.GetShipmentsByOrderId(order.Id))
                {
                    foreach (var shipmentItem in _shipmentService.GetShipmentItemsByShipmentId(shipment.Id))
                    {
                        var product = _orderService.GetProductByOrderItemId(shipmentItem.OrderItemId);
                        if (product == null)
                            continue;

                        _productService.ReverseBookedInventory(product, shipmentItem,
                            string.Format("The stock quantity has been increased by deleting the order #{0}", order.Id, order.GetType().Name));
                    }
                }

                foreach (var orderItem in _orderService.GetOrderItems(order.Id))
                {
                    var product = _productService.GetProductById(orderItem.ProductId);

                    _productService.AdjustInventory(product, orderItem.Quantity, orderItem.AttributesXml,
                        string.Format("The stock quantity has been increased by deleting the order #{0}", order.Id));
                }
            }

            AddOrderNote(order, "Order has been deleted");

            _orderService.DeleteOrder(order);
        }

        public virtual void Ship(Shipment shipment, bool notifyCustomer)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            if (shipment.ShippedDateUtc.HasValue)
                throw new Exception("This shipment is already shipped");

            shipment.ShippedDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            foreach (var item in _shipmentService.GetShipmentItemsByShipmentId(shipment.Id))
            {
                var product = _orderService.GetProductByOrderItemId(item.OrderItemId);

                if (product is null)
                    continue;

                _productService.BookReservedInventory(product, item.WarehouseId, -item.Quantity,
                    string.Format("The stock quantity has been reduced when an order item of the order #{0} was shipped", shipment.OrderId));
            }

            if (_orderService.HasItemsToAddToShipment(order) || _orderService.HasItemsToShip(order))
                order.ShippingStatusId = (int)ShippingStatus.PartiallyShipped;
            else
                order.ShippingStatusId = (int)ShippingStatus.Shipped;
            _orderService.UpdateOrder(order);

            AddOrderNote(order, $"Shipment# {shipment.Id} has been sent");

            if (notifyCustomer)
            {
                var queuedEmailIds = _workflowMessageService.SendShipmentSentCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailIds.Any())
                    AddOrderNote(order, $"\"Shipped\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");
            }

            //event
            //_eventPublisher.PublishShipmentSent(shipment);

            //check order status
            CheckOrderStatus(order);
        }

        public virtual void Deliver(Shipment shipment, bool notifyCustomer)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = _orderService.GetOrderById(shipment.OrderId);
            if (order == null)
                throw new Exception("Order cannot be loaded");

            if (!shipment.ShippedDateUtc.HasValue)
                throw new Exception("This shipment is not shipped yet");

            if (shipment.DeliveryDateUtc.HasValue)
                throw new Exception("This shipment is already delivered");

            shipment.DeliveryDateUtc = DateTime.UtcNow;
            _shipmentService.UpdateShipment(shipment);

            if (!_orderService.HasItemsToAddToShipment(order) && !_orderService.HasItemsToShip(order) && !_orderService.HasItemsToDeliver(order))
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
            _orderService.UpdateOrder(order);

            AddOrderNote(order, $"Shipment# {shipment.Id} has been delivered");

            if (notifyCustomer)
            {
                var queuedEmailIds = _workflowMessageService.SendShipmentDeliveredCustomerNotification(shipment, order.CustomerLanguageId);
                if (queuedEmailIds.Any())
                    AddOrderNote(order, $"\"Delivered\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");
            }

            //event
            //_eventPublisher.PublishShipmentDelivered(shipment);

            //check order status
            CheckOrderStatus(order);
        }

        public virtual IList<string> Void(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanVoid(order))
                throw new InovatiqaException("Cannot do void for order.");

            var request = new VoidPaymentRequest();
            VoidPaymentResult result = null;
            try
            {
                request.Order = order;
                result = _paymentService.Void(request);

                if (result.Success)
                {
                    order.PaymentStatusId = (int)result.NewPaymentStatus;
                    _orderService.UpdateOrder(order);

                    AddOrderNote(order, "Order has been voided");

                    CheckOrderStatus(order);

                    //raise event       
                    //_eventPublisher.Publish(new OrderVoidedEvent(order));
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new VoidPaymentResult();
                result.AddError($"Error: {exc.Message}. Full exception: {exc}");
            }

            //process errors
            var error = string.Empty;
            for (var i = 0; i < result.Errors.Count; i++)
            {
                error += $"Error {i}: {result.Errors[i]}";
                if (i != result.Errors.Count - 1)
                    error += ". ";
            }

            if (string.IsNullOrEmpty(error))
                return result.Errors;
            AddOrderNote(order, $"Unable to voiding order. {error}");

            var logError = $"Error voiding order #{order.Id}. Error: {error}";
            _loggerService.InsertLog((int)LogLevel.Error, logError, logError);
            return result.Errors;
        }

        public virtual CapturePaymentResult CaptureShipment(string shipmentAuthorizationId, Shipment shipment)
        {
            var request = new CapturePaymentRequest();
            CapturePaymentResult result = null;
            var order = _orderService.GetOrderById(shipment.OrderId);

            try
            {
                request.ShipmentAuthorizationId = shipmentAuthorizationId;
                result = _paymentService.Capture(request);

                if (result.Success)
                {
                    if (result.NewPaymentStatus == PaymentStatus.Paid)
                    {
                        _shipmentService.UpdateShipment(shipment);
                        if (shipment.PaymentStatusId == (int)PaymentStatus.PartiallyPaid)
                            AddOrderNote(order, $"\"Invoice partially paid\". Order #: {order.CustomOrderNumber}. Invoice #: {shipment.Id}. Amount paid: {_priceFormatter.FormatPrice(decimal.Parse(shipment.AmountPaid.ToString()))}. Date paid: {DateTime.UtcNow}");
                        else if (shipment.PaymentStatusId == (int)PaymentStatus.Paid)
                            AddOrderNote(order, $"\"Invoice paid\". Invoice #: {shipment.Id}. Amount paid: {_priceFormatter.FormatPrice(decimal.Parse(shipment.AmountPaid.ToString()))}. Date paid: {DateTime.UtcNow}");

                        var orderTotal = order.OrderTotal;
                        var shipmentsTotal = 0.0m;

                        var shipments = _shipmentService.GetShipmentsByOrderId(order.Id);
                        foreach(var s in shipments)
                        {
                            shipmentsTotal += s.AmountPaid != null ? decimal.Parse(s.AmountPaid.ToString()) : 0.0m;
                        }

                        if(orderTotal == shipmentsTotal)
                        {
                            order.PaymentStatusId = (int)PaymentStatus.Paid;
                            _orderService.UpdateOrder(order);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                if (result == null)
                    result = new CapturePaymentResult();
                result.AddError($"Error: {exc.Message}. Full exception: {exc}");
                _loggerService.Error(exc.Message, exc);
            }

            if (result.Success)
                return result;

            var logError = result.Errors.Aggregate("Error while capturing invoice. ",
                (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
            var customer = _customerService.GetCustomerById(order.CustomerId);
            _loggerService.Error(logError, customer: customer);

            return result;
        }

        #endregion
    }
}