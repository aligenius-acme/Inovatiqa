using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Inovatiqa.Services.Messages
{
    public partial class MessageTokenProviderService : IMessageTokenProviderService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IProductService _productService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IAddressAttributeFormatterService _addressAttributeFormatterService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDateTimeHelperService _dateTimeHelperService;
        private readonly IShipmentService _shipmentService;

        #endregion

        #region Ctor

        public MessageTokenProviderService(ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IProductService productService,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IAddressAttributeFormatterService addressAttributeFormatterService,
            IPaymentService paymentService,
            IOrderService orderService,
            IPriceFormatter priceFormatter,
            IShipmentService shipmentService,
            IDateTimeHelperService dateTimeHelperService)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _productService = productService;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _addressAttributeFormatterService = addressAttributeFormatterService;
            _paymentService = paymentService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _dateTimeHelperService = dateTimeHelperService;
            _shipmentService = shipmentService;
        }

        #endregion

        #region Allowed tokens


        #endregion

        #region Utilities

        protected virtual string ProductListToHtmlTable(Shipment shipment, int languageId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{InovatiqaDefaults.Color1};text-align:center;\">");
            sb.AppendLine($"<th>Name</th>");
            sb.AppendLine($"<th>Quantity</th>");
            sb.AppendLine("</tr>");

            var table = _shipmentService.GetShipmentItemsByShipmentId(shipment.Id);
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var si = table[i];
                var orderItem = _orderService.GetOrderItemById(si.OrderItemId);

                if (orderItem == null)
                    continue;

                var product = _productService.GetProductById(orderItem?.ProductId ?? 0);

                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {InovatiqaDefaults.Color2};text-align: center;\">");

                var productName = product.Name;

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));


                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }

                if (InovatiqaDefaults.ShowSkuOnProductDetailsPage)
                {
                    var sku = _productService.FormatSku(product, orderItem.AttributesXml);
                    if (!string.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format("SKU: {0}", WebUtility.HtmlEncode(sku)));
                    }
                }

                sb.AppendLine("</td>");

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{si.Quantity}</td>");

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }

        protected virtual void WriteTotals(Order order, Language language, StringBuilder sb)
        {
            string cusSubTotal;
            var displaySubTotalDiscount = false;
            var cusSubTotalDiscount = string.Empty;
            var languageId = language.Id;
            if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax && !InovatiqaDefaults.ForceTaxExclusionFromOrderSubtotal)
            {
                var orderSubtotalInclTaxInCustomerCurrency = order.OrderSubtotalInclTax;
                cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency);

                var orderSubTotalDiscountInclTaxInCustomerCurrency = order.OrderSubTotalDiscountInclTax;
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                {
                    cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency);
                    displaySubTotalDiscount = true;
                }
            }
            else
            {
                var orderSubtotalExclTaxInCustomerCurrency = order.OrderSubtotalExclTax;
                cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency);

                var orderSubTotalDiscountExclTaxInCustomerCurrency = order.OrderSubTotalDiscountExclTax;
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                {
                    cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency);
                    displaySubTotalDiscount = true;
                }
            }

            string cusShipTotal;
            string cusPaymentMethodAdditionalFee;
            var taxRates = new SortedDictionary<decimal, decimal>();
            var cusTaxTotal = string.Empty;
            var cusDiscount = string.Empty;
            if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
            {
                var orderShippingInclTaxInCustomerCurrency = order.OrderShippingInclTax;
                cusShipTotal = _priceFormatter.FormatPrice(orderShippingInclTaxInCustomerCurrency);

                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = order.PaymentMethodAdditionalFeeInclTax;
                cusPaymentMethodAdditionalFee = _priceFormatter.FormatPrice(paymentMethodAdditionalFeeInclTaxInCustomerCurrency);
            }
            else
            {
                var orderShippingExclTaxInCustomerCurrency = order.OrderShippingExclTax;
                cusShipTotal = _priceFormatter.FormatPrice(orderShippingExclTaxInCustomerCurrency);

                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = order.PaymentMethodAdditionalFeeExclTax;
                cusPaymentMethodAdditionalFee = _priceFormatter.FormatPrice(paymentMethodAdditionalFeeExclTaxInCustomerCurrency);
            }

            var displayShipping = order.ShippingStatusId != (int)ShippingStatus.ShippingNotRequired;

            var displayPaymentMethodFee = order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

            bool displayTax = false;
            bool displayTaxRates;
            if (InovatiqaDefaults.HideTaxInOrderSummary && order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && InovatiqaDefaults.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
            }

            var displayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = order.OrderDiscount;
                cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency);
                displayDiscount = true;
            }

            var orderTotalInCustomerCurrency = order.OrderTotal;
            var cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency);

            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{"Sub-Total:"}</strong></td> <td style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotal}</strong></td></tr>");

            if (displaySubTotalDiscount)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{"Discount:"}</strong></td> <td style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotalDiscount}</strong></td></tr>");
            }

            if (displayShipping)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{"Shipping:"}</strong></td> <td style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{cusShipTotal}</strong></td></tr>");
            }

            if (displayPaymentMethodFee)
            {
                var paymentMethodFeeTitle = "Payment method additional fee:";
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{paymentMethodFeeTitle}</strong></td> <td style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{cusPaymentMethodAdditionalFee}</strong></td></tr>");
            }

            if (displayTax)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{"Tax:"}</strong></td> <td style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{cusTaxTotal}</strong></td></tr>");
            }

            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{"Order Total:"}</strong></td> <td style=\"background-color: {InovatiqaDefaults.Color3};padding:0.6em 0.4 em;\"><strong>{cusTotal}</strong></td></tr>");
        }

        protected virtual string ProductListToHtmlTable(Order order, int languageId, int vendorId)
        {
            Language language = new Language();
            language.DefaultCurrencyId = InovatiqaDefaults.LanguageDefaultCurrencyId;
            language.DisplayOrder = InovatiqaDefaults.DisplayOrder;
            language.FlagImageFileName = InovatiqaDefaults.FlagImageFileName;
            language.LimitedToStores = InovatiqaDefaults.LimitedToStores;
            language.Name = InovatiqaDefaults.LanguageName;
            language.Rtl = InovatiqaDefaults.Rtl;
            language.UniqueSeoCode = InovatiqaDefaults.UniqueSeoCode;

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{InovatiqaDefaults.Color1};text-align:center;\">");
            sb.AppendLine($"<th>{"Name"}</th>");
            sb.AppendLine($"<th>{"Price"}</th>");
            sb.AppendLine($"<th>{"Quantity"}</th>");
            sb.AppendLine($"<th>{"Total"}</th>");
            sb.AppendLine("</tr>");

            var table = _orderService.GetOrderItems(order.Id, vendorId: vendorId);
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];

                var product = _productService.GetProductById(orderItem.ProductId);

                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {InovatiqaDefaults.Color2};text-align: center;\">");

                var productName = product.Name;

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                
                if (InovatiqaDefaults.ShowSkuOnProductDetailsPage)
                {
                    var sku = _productService.FormatSku(product, orderItem.AttributesXml);
                    if (!string.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format("SKU: {0}", WebUtility.HtmlEncode(sku)));
                    }
                }

                sb.AppendLine("</td>");

                string unitPriceStr;
                if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                {
                    var unitPriceInclTaxInCustomerCurrency = orderItem.UnitPriceInclTax;
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency);
                }
                else
                {
                    var unitPriceExclTaxInCustomerCurrency = orderItem.UnitPriceExclTax;
                    unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}</td>");

                string priceStr;
                if (order.CustomerTaxDisplayTypeId == (int)TaxDisplayType.IncludingTax)
                {
                    var priceInclTaxInCustomerCurrency = orderItem.PriceInclTax;
                    priceStr = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency);
                }
                else
                {
                    var priceExclTaxInCustomerCurrency = orderItem.PriceExclTax;
                    priceStr = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

                sb.AppendLine("</tr>");
            }

            if (vendorId == 0)
            {
                if (!string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                {
                    sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
                    sb.AppendLine(order.CheckoutAttributeDescription);
                    sb.AppendLine("</td></tr>");
                }
                WriteTotals(order, language, sb);
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }

        protected virtual string RouteUrl(int storeId = 0, string routeName = null, object routeValues = null)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));

            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            return Uri.EscapeUriString(WebUtility.UrlDecode($"{InovatiqaDefaults.StoreUrl.TrimEnd('/')}{url}"));
        }

        #endregion

        #region Methods

        public virtual void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote)
        {
            var order = _orderService.GetOrderById(orderNote.OrderId);

            tokens.Add(new Token("Order.NewNoteText", _orderService.FormatOrderNoteText(orderNote), true));
            //var orderNoteAttachmentUrl = RouteUrl(order.StoreId, "GetOrderNoteFile", new { ordernoteid = orderNote.Id });
            //tokens.Add(new Token("Order.OrderNoteAttachmentUrl", orderNoteAttachmentUrl, true));

            //event notification
            //_eventPublisher.EntityTokensAdded(orderNote, tokens);
        }
        // Email Format CheckPoint By hamza
        public virtual void AddOrderTokens(IList<Token> tokens, Order order, int languageId, int vendorId = 0)
        {
            Address orderAddress(Order o) => _addressService.GetAddressById((o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) ?? 0);

            var billingAddress = _addressService.GetAddressById(order.BillingAddressId);

            tokens.Add(new Token("Order.OrderId", order.Id));
            tokens.Add(new Token("Order.OrderNumber", order.CustomOrderNumber));

            tokens.Add(new Token("Order.CustomerFullName", $"{billingAddress.FirstName} {billingAddress.LastName}"));
            tokens.Add(new Token("Order.CustomerEmail", billingAddress.Email));

            tokens.Add(new Token("Order.BillingFirstName", billingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", billingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", billingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", billingAddress.Email));
            tokens.Add(new Token("Order.BillingFaxNumber", billingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", billingAddress.Company));
            tokens.Add(new Token("Order.BillingAddress1", billingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", billingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", billingAddress.City));
            tokens.Add(new Token("Order.BillingCounty", billingAddress.County));
            tokens.Add(new Token("Order.BillingStateProvince", _stateProvinceService.GetStateProvinceByAddress(billingAddress) is StateProvince billingStateProvince ? billingStateProvince.Name : string.Empty));
            tokens.Add(new Token("Order.BillingZipPostalCode", billingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", _countryService.GetCountryByAddress(billingAddress) is Country billingCountry ? billingCountry.Name : string.Empty));
            tokens.Add(new Token("Order.BillingCustomAttributes", _addressAttributeFormatterService.FormatAttributes(billingAddress.CustomAttributes), true));

            tokens.Add(new Token("Order.Shippable", !string.IsNullOrEmpty(order.ShippingMethod)));
            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.PickupInStore", order.PickupInStore));
            tokens.Add(new Token("Order.ShippingFirstName", orderAddress(order)?.FirstName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingLastName", orderAddress(order)?.LastName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingPhoneNumber", orderAddress(order)?.PhoneNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingEmail", orderAddress(order)?.Email ?? string.Empty));
            tokens.Add(new Token("Order.ShippingFaxNumber", orderAddress(order)?.FaxNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCompany", orderAddress(order)?.Company ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress1", orderAddress(order)?.Address1 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress2", orderAddress(order)?.Address2 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCity", orderAddress(order)?.City ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCounty", orderAddress(order)?.County ?? string.Empty));
            tokens.Add(new Token("Order.ShippingStateProvince", _stateProvinceService.GetStateProvinceByAddress(orderAddress(order)) is StateProvince shippingStateProvince ? shippingStateProvince.Name : string.Empty));
            tokens.Add(new Token("Order.ShippingZipPostalCode", orderAddress(order)?.ZipPostalCode ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCountry", _countryService.GetCountryByAddress(orderAddress(order)) is Country orderCountry ? orderCountry.Name : string.Empty));
            tokens.Add(new Token("Order.ShippingCustomAttributes", _addressAttributeFormatterService.FormatAttributes(orderAddress(order)?.CustomAttributes ?? string.Empty), true));

            var paymentMethodName = InovatiqaDefaults.SystemName;
            tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
            tokens.Add(new Token("Order.VatNumber", order.VatNumber));
            var sbCustomValues = new StringBuilder();
            var customValues = _paymentService.DeserializeCustomValues(order);
            if (customValues != null)
            {
                foreach (var item in customValues)
                {
                    sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : string.Empty));
                    sbCustomValues.Append("<br />");
                }
            }

            tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));

            tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order, languageId, vendorId), true));

            Language language = new Language();
            language.DefaultCurrencyId = InovatiqaDefaults.LanguageDefaultCurrencyId;
            language.DisplayOrder = InovatiqaDefaults.DisplayOrder;
            language.FlagImageFileName = InovatiqaDefaults.FlagImageFileName;
            language.LimitedToStores = InovatiqaDefaults.LimitedToStores;
            language.Name = InovatiqaDefaults.LanguageName;
            language.Rtl = InovatiqaDefaults.Rtl;
            language.UniqueSeoCode = InovatiqaDefaults.UniqueSeoCode;

            if (language != null && !string.IsNullOrEmpty(language.LanguageCulture))
            {
                var customer = _customerService.GetCustomerById(order.CustomerId);
                var createdOn = _dateTimeHelperService.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, _dateTimeHelperService.GetCustomerTimeZone(customer));
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
            }
            else
            {
                tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
            }

            var orderUrl = RouteUrl(order.StoreId, "OrderDetails", new { orderId = order.Id });
            tokens.Add(new Token("Order.OrderURLForCustomer", orderUrl, true));

            ////event notification
            //_eventPublisher.EntityTokensAdded(order, tokens);
        }

        public virtual void AddCustomerTokens(IList<Token> tokens, int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentOutOfRangeException(nameof(customerId));

            var customer = _customerService.GetCustomerById(customerId);

            AddCustomerTokens(tokens, customer);
        }

        public virtual void AddCustomerTokens(IList<Token> tokens, Customer customer)
        {
            tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", _customerService.GetCustomerFullName(customer)));
            tokens.Add(new Token("Customer.FirstName", _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id)));
            tokens.Add(new Token("Customer.LastName", _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id)));


            var passwordRecoveryUrl = RouteUrl(routeName: "PasswordRecoveryConfirm", routeValues: new { token = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PasswordRecoveryTokenAttribute, customer.Id), customerGuid = customer.CustomerGuid});
            //var accountActivationUrl = RouteUrl(routeName: "AccountActivation", routeValues: new { token = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.AccountActivationTokenAttribute, customer.Id), customerGuid = customer.CustomerGuid });
            var accountActivationUrl = RouteUrl(routeName: "AccountActivation", routeValues: new { customerGuid = customer.CustomerGuid });
            var emailRevalidationUrl = RouteUrl(routeName: "EmailRevalidation", routeValues: new { token = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.EmailRevalidationTokenAttribute, customer.Id), guid = customer.CustomerGuid });
            var wishlistUrl = RouteUrl(routeName: "Wishlist", routeValues: new { customerGuid = customer.CustomerGuid });
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Customer.EmailRevalidationURL", emailRevalidationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));

            //_eventPublisher.EntityTokensAdded(customer, tokens);
        }

        public virtual void AddStoreTokens(IList<Token> tokens, EmailAccount emailAccount)
        {
            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            tokens.Add(new Token("Store.Name", InovatiqaDefaults.CurrentStoreName));
            tokens.Add(new Token("Store.URL", InovatiqaDefaults.StoreUrl, true));
            tokens.Add(new Token("Store.Email", emailAccount.Email));
            tokens.Add(new Token("Store.CompanyName", InovatiqaDefaults.CurrentStoreName));
            tokens.Add(new Token("Store.CompanyAddress", InovatiqaDefaults.CompanyAddress));
            tokens.Add(new Token("Store.CompanyPhoneNumber", InovatiqaDefaults.CompanyPhoneNo));

            tokens.Add(new Token("Facebook.URL", InovatiqaDefaults.FacebookLink));
            tokens.Add(new Token("Twitter.URL", InovatiqaDefaults.TwitterLink));
            tokens.Add(new Token("Linkedin.URL", InovatiqaDefaults.LinkedInLink));

            //_eventPublisher.EntityTokensAdded(store, tokens);
        }

        public virtual void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview)
        {
            tokens.Add(new Token("ProductReview.ProductName", _productService.GetProductById(productReview.ProductId)?.Name));
            tokens.Add(new Token("ProductReview.Title", productReview.Title));
            tokens.Add(new Token("ProductReview.IsApproved", productReview.IsApproved));
            tokens.Add(new Token("ProductReview.ReviewText", productReview.ReviewText));
            tokens.Add(new Token("ProductReview.ReplyText", productReview.ReplyText));

            //_eventPublisher.EntityTokensAdded(productReview, tokens);
        }

        public virtual void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem)
        {
            var product = _productService.GetProductById(orderItem.ProductId);

            tokens.Add(new Token("ReturnRequest.CustomNumber", returnRequest.CustomNumber));
            tokens.Add(new Token("ReturnRequest.OrderId", orderItem.OrderId));
            tokens.Add(new Token("ReturnRequest.Product.Quantity", returnRequest.Quantity));
            tokens.Add(new Token("ReturnRequest.Product.Name", product.Name));
            tokens.Add(new Token("ReturnRequest.Reason", returnRequest.ReasonForReturn));
            tokens.Add(new Token("ReturnRequest.RequestedAction", returnRequest.RequestedAction));
            tokens.Add(new Token("ReturnRequest.CustomerComment", HtmlHelper.FormatText(returnRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.StaffNotes", HtmlHelper.FormatText(returnRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.Status", ((ReturnRequestStatus)returnRequest.ReturnRequestStatusId).ToString()));

            //event notification
            //_eventPublisher.EntityTokensAdded(returnRequest, tokens);
        }

        public virtual void AddOrderRefundedTokens(IList<Token> tokens, Order order, decimal refundedAmount)
        {
            var primaryStoreCurrencyCode = InovatiqaDefaults.CurrencyCode;
            var refundedAmountStr = _priceFormatter.FormatPrice(refundedAmount);

            tokens.Add(new Token("Order.AmountRefunded", refundedAmountStr));

            //event notification
            //_eventPublisher.EntityTokensAdded(order, tokens);
        }

        public virtual void AddShipmentTokens(IList<Token> tokens, Shipment shipment, int languageId)
        {
            tokens.Add(new Token("Shipment.ShipmentNumber", shipment.Id));
            tokens.Add(new Token("Shipment.TrackingNumber", shipment.TrackingNumber));
            var trackingNumberUrl = string.Empty;
            if (!string.IsNullOrEmpty(shipment.TrackingNumber))
            {
                var shipmentTracker = _shipmentService.GetShipmentTracker(shipment);
                if (shipmentTracker != null)
                    trackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
            }

            tokens.Add(new Token("Shipment.TrackingNumberURL", trackingNumberUrl, true));
            tokens.Add(new Token("Shipment.Product(s)", ProductListToHtmlTable(shipment, languageId), true));

            var shipmentUrl = RouteUrl(_orderService.GetOrderById(shipment.OrderId).StoreId, "ShipmentDetails", new { shipmentId = shipment.Id });
            tokens.Add(new Token("Shipment.URLForCustomer", shipmentUrl, true));

            //event notification
            //_eventPublisher.EntityTokensAdded(shipment, tokens);
        }

        #endregion
    }
}