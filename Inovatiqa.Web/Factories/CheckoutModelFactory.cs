using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Checkout;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Web.Factories
{
    public partial class CheckoutModelFactory : ICheckoutModelFactory
    {
        #region Fields

        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContextService _workContextService;
        private readonly ICountryService _countryService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IShippingService _shippingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPaymentService _paymentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParserService _productAttributeParserService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPriceCalculationService _priceCalculationService;

        #endregion

        #region Ctor

        public CheckoutModelFactory(IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWorkContextService workContextService,
            ICountryService countryService,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IShippingService shippingService,
            IGenericAttributeService genericAttributeService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceFormatter priceFormatter,
            IPaymentService paymentService,
            IOrderProcessingService orderProcessingService,
            ICurrencyService currencyService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            IProductAttributeParserService productAttributeParserService,
            IPriceCalculationService priceCalculationService)
        {
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _urlRecordService = urlRecordService;
            _workContextService = workContextService;
            _countryService = countryService;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _shippingService = shippingService;
            _genericAttributeService = genericAttributeService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceFormatter = priceFormatter;
            _paymentService = paymentService;
            _orderProcessingService = orderProcessingService;
            _currencyService = currencyService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _productAttributeParserService = productAttributeParserService;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Utilities

        protected virtual CheckoutPickupPointsModel PrepareCheckoutPickupPointsModel(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutPickupPointsModel()
            {
                AllowPickupInStore = InovatiqaDefaults.AllowPickupInStore
            };

            return model;
        }

        #endregion

        #region Methods

        public virtual OnePageCheckoutModel PrepareOnePageCheckoutModel(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var model = new OnePageCheckoutModel
            {
                ShippingRequired = _shoppingCartService.ShoppingCartRequiresShipping(cart),
                DisableBillingAddressCheckoutStep = InovatiqaDefaults.DisableBillingAddressCheckoutStep && _customerService.GetAddressesByCustomerId(_workContextService.CurrentCustomer.Id).Any(),
                BillingAddress = PrepareBillingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true)
            };

            decimal orderTotals = 0;
            foreach(var product in cart)
            {
                var Product = _productService.GetProductById(product.ProductId);
                var temp = _productAttributeParserService.ParseProductAttributeValues(product.AttributesXml);
                var checkoutProduct = new CheckoutProducts
                {
                    Id = product.Id,
                    ProductId = product.ProductId,
                    Name = product.Product.Name,
                    //08/06/22 add SeName for required in checkout page also inject _urlRecordService and ProductAttributes
                    SeName = _urlRecordService.GetActiveSlug(product.ProductId, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                    StartRating = 5,
                    Price = _priceFormatter.FormatPrice(_priceCalculationService.GetFinalPrice(Product, _workContextService.CurrentCustomer, temp.First().PriceAdjustment, false, product.Quantity)).Replace("$", ""),
                    OldPriceEnabled = Product.OldPrice > Product.Price,
                    OldPrice = _priceFormatter.FormatPrice(Product.OldPrice).Replace("$", ""),
                    DefaultPictureModel = PrepareProductOverviewPictureModel(Product, null),
                    Attributes = { product.Quantity.ToString(), temp.First().Name }
                };
                orderTotals += Convert.ToDecimal(checkoutProduct.Price) * product.Quantity;
                model.orderSummaryBox.OrderSummaryProducts.Add(checkoutProduct);

            }

            //model.orderSummaryBox.SubTotal = model.orderSummaryBox.OrderSummaryProducts.Sum(osp => Convert.ToDecimal(osp.Price)).ToString();
            // 07-06-2022 ALI AHMAD - DUE TO ORDER SUMMARY TOTALS WERE NOT CORRECT
            model.orderSummaryBox.SubTotal = orderTotals.ToString();
            return model;
        }
        public virtual PictureModel PrepareProductOverviewPictureModel(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = product.Name;
            var pictureSize = productThumbPictureSize ?? InovatiqaDefaults.ProductThumbPictureSize;

            var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
            var pictureModel = new PictureModel
            {
                ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize),
                FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
                    ? picture.TitleAttribute
                    : string.Format("Show details for {0}",
                        productName),
                AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
                    ? picture.AltAttribute
                    : string.Format("Picture of {0}",
                        productName)
            };

            return pictureModel;
        }
        public virtual CheckoutBillingAddressModel PrepareBillingAddressModel(IList<ShoppingCartItem> cart,
            int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false,
            string overrideAttributesXml = "")
        {
            var model = new CheckoutBillingAddressModel
            {
                ShipToSameAddressAllowed = InovatiqaDefaults.ShipToSameAddress && _shoppingCartService.ShoppingCartRequiresShipping(cart),

                ShipToSameAddress = !InovatiqaDefaults.DisableBillingAddressCheckoutStep
            };

            var customer = _workContextService.CurrentCustomer;

            var addresses = _customerService.GetAddressesByCustomerId(customer.Id)
                .Where(a => _countryService.GetCountryByAddress(a) is Country country &&
                    (
                    country.Published &&
                    country.AllowsBilling))
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressModelFactory.PrepareAddressModel(addressModel,
                    address: address,
                    excludeProperties: false);

                if (_addressService.IsAddressValid(address))
                {
                    model.ExistingAddresses.Add(addressModel);
                }
                else
                {
                    model.InvalidExistingAddresses.Add(addressModel);
                }
            }

            model.BillingNewAddress.CountryId = selectedCountryId;
            _addressModelFactory.PrepareAddressModel(model.BillingNewAddress,
                address: null,
                excludeProperties: false,
                loadCountries: () => _countryService.GetAllCountriesForBilling(InovatiqaDefaults.LanguageId),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: customer,
                overrideAttributesXml: overrideAttributesXml);
            return model;
        }

        public virtual CheckoutShippingMethodModel PrepareShippingMethodModel(IList<ShoppingCartItem> cart, Address shippingAddress)
        {
            var model = new CheckoutShippingMethodModel()
            {
                DisplayPickupInStore = InovatiqaDefaults.DisplayPickupInStoreOnShippingMethodPage
            };

            if (InovatiqaDefaults.DisplayPickupInStoreOnShippingMethodPage)
                model.PickupPointsModel = PrepareCheckoutPickupPointsModel(cart);

            var customer = _workContextService.CurrentCustomer;

            var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, shippingAddress, customer, storeId: InovatiqaDefaults.StoreId);
            if (getShippingOptionResponse.Success)
            {
                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id,
                                                       InovatiqaDefaults.OfferedShippingOptionsAttribute,
                                                       getShippingOptionResponse.ShippingOptions,
                                                       InovatiqaDefaults.StoreId);

                foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                {
                    var soModel = new CheckoutShippingMethodModel.ShippingMethodModel
                    {
                        Name = shippingOption.Name,
                        Description = shippingOption.Description,
                        ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                        ShippingOption = shippingOption,
                    };

                    var shippingTotal = _orderTotalCalculationService.AdjustShippingRate(shippingOption.Rate, cart, out var _, shippingOption.IsPickupInStore);

                    var rateBase = shippingTotal;
                    var rate = rateBase;
                    soModel.Fee = _priceFormatter.FormatPrice(rate);

                    model.ShippingMethods.Add(soModel);
                }

                var selectedShippingOption = _genericAttributeService.GetAttribute<ShippingOption>(customer,
                        InovatiqaDefaults.SelectedShippingOptionAttribute, InovatiqaDefaults.StoreId);
                if (selectedShippingOption != null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.ToList()
                        .Find(so =>
                           !string.IsNullOrEmpty(so.Name) &&
                           so.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) &&
                           !string.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) &&
                           so.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }
                if (model.ShippingMethods.FirstOrDefault(so => so.Selected) == null)
                {
                    var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
                    if (shippingOptionToSelect != null)
                    {
                        shippingOptionToSelect.Selected = true;
                    }
                }

                if (InovatiqaDefaults.NotifyCustomerAboutShippingFromMultipleLocations)
                {
                    model.NotifyCustomerAboutShippingFromMultipleLocations = getShippingOptionResponse.ShippingFromMultipleLocations;
                }
            }
            else
            {
                foreach (var error in getShippingOptionResponse.Errors)
                    model.Warnings.Add(error);
            }

            return model;
        }

        public virtual CheckoutPaymentMethodModel PreparePaymentMethodModel(IList<ShoppingCartItem> cart, int filterByCountryId)
        {
            var model = new CheckoutPaymentMethodModel();

            var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
            {
                Name = "Credit Card",
                Description = "Pay by credit card using Square",
                PaymentMethodSystemName = InovatiqaDefaults.SystemName,
                LogoUrl = ""
            };
            var isB2BCustomer = _customerService.IsB2B(_workContextService.CurrentCustomer);
            if (isB2BCustomer)
            {
                var isPOCustomer = _customerService.IsPO(_workContextService.CurrentCustomer);
                if (isPOCustomer)
                {
                    var pmModelPO = new CheckoutPaymentMethodModel.PaymentMethodModel
                    {
                        Name = "Purchase Order",
                        Description = "Pay by purchase order (PO) number",
                        PaymentMethodSystemName = InovatiqaDefaults.PurchaseOrderPaymentName,
                        LogoUrl = ""
                    };
                    model.PaymentMethods.Add(pmModelPO);
                    // add by hamza for displaying both payment methods
                    //model.PaymentMethods.Add(pmModel);
                }
                else
                    model.PaymentMethods.Add(pmModel);
            }
            else
                model.PaymentMethods.Add(pmModel);

            var paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, InovatiqaDefaults.SystemName);
            var rateBase = paymentMethodAdditionalFee;
            var rate = rateBase;
            if (rate > decimal.Zero)
                pmModel.Fee = _priceFormatter.FormatPrice(rate);

            //model.PaymentMethods.Add(pmModel);

            var selectedPaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContextService.CurrentCustomer,
                InovatiqaDefaults.SelectedPaymentMethodAttribute, InovatiqaDefaults.StoreId);
            if (!string.IsNullOrEmpty(selectedPaymentMethodSystemName))
            {
                var paymentMethodToSelect = model.PaymentMethods.ToList()
                    .Find(pm => pm.PaymentMethodSystemName.Equals(selectedPaymentMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }
            if (model.PaymentMethods.FirstOrDefault(so => so.Selected) == null)
            {
                var paymentMethodToSelect = model.PaymentMethods.FirstOrDefault();
                if (paymentMethodToSelect != null)
                    paymentMethodToSelect.Selected = true;
            }

            return model;
        }

        public virtual CheckoutPaymentInfoModel PreparePaymentInfoModel(string paymentMethod)
        {
            if (paymentMethod == InovatiqaDefaults.SystemName)
            {
                return new CheckoutPaymentInfoModel
                {
                    PaymentViewComponentName = InovatiqaDefaults.VIEW_COMPONENT_NAME,
                    DisplayOrderTotals = InovatiqaDefaults.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab
                };
            }
            else if (paymentMethod == InovatiqaDefaults.PurchaseOrderPaymentName)
            {
                return new CheckoutPaymentInfoModel
                {
                    PaymentViewComponentName = InovatiqaDefaults.VIEW_COMPONENT_NAME_PO,
                    DisplayOrderTotals = InovatiqaDefaults.OnePageCheckoutDisplayOrderTotalsOnPaymentInfoTab
                };
            }
            else
                return null;
        }

        public virtual CheckoutConfirmModel PrepareConfirmOrderModel(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutConfirmModel
            {
                TermsOfServiceOnOrderConfirmPage = InovatiqaDefaults.TermsOfServiceOnOrderConfirmPage,
                TermsOfServicePopup = InovatiqaDefaults.PopupForTermsOfServiceLinks
            };

            var minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                var minOrderTotalAmount = InovatiqaDefaults.MinOrderTotalAmount;
                model.MinOrderTotalWarning = string.Format("Minimum order total amount is {0}", _priceFormatter.FormatPrice(minOrderTotalAmount));
            }
            return model;
        }

        public virtual CheckoutShippingAddressModel PrepareShippingAddressModel(IList<ShoppingCartItem> cart,
            int? selectedCountryId = null, bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var model = new CheckoutShippingAddressModel()
            {
                DisplayPickupInStore = !InovatiqaDefaults.DisplayPickupInStoreOnShippingMethodPage
            };

            if (!InovatiqaDefaults.DisplayPickupInStoreOnShippingMethodPage)
                model.PickupPointsModel = PrepareCheckoutPickupPointsModel(cart);

            var customer = _workContextService.CurrentCustomer;

            var addresses = _customerService.GetAddressesByCustomerId(customer.Id)
                .Where(a => _countryService.GetCountryByAddress(a) is Country country &&
                    (
                    country.Published &&
                    country.AllowsShipping))
                .ToList();
            foreach (var address in addresses)
            {
                var addressModel = new AddressModel();
                _addressModelFactory.PrepareAddressModel(addressModel,
                    address: address,
                    excludeProperties: false);

                if (_addressService.IsAddressValid(address))
                {
                    model.ExistingAddresses.Add(addressModel);
                }
                else
                {
                    model.InvalidExistingAddresses.Add(addressModel);
                }
            }

            model.ShippingNewAddress.CountryId = selectedCountryId;
            _addressModelFactory.PrepareAddressModel(model.ShippingNewAddress,
                address: null,
                excludeProperties: false,
                loadCountries: () => _countryService.GetAllCountriesForShipping(InovatiqaDefaults.LanguageId),
                prePopulateWithCustomerFields: prePopulateNewAddressWithCustomerFields,
                customer: customer,
                overrideAttributesXml: overrideAttributesXml);

            return model;
        }

        public virtual CheckoutProgressModel PrepareCheckoutProgressModel(CheckoutProgressStep step)
        {
            var model = new CheckoutProgressModel { CheckoutProgressStep = step };
            return model;
        }

        public virtual CheckoutCompletedModel PrepareCheckoutCompletedModel(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var model = new CheckoutCompletedModel
            {
                OrderId = order.Id,
                OnePageCheckoutEnabled = InovatiqaDefaults.OnePageCheckoutEnabled,
                CustomOrderNumber = order.CustomOrderNumber
            };

            return model;
        }

        #endregion
    }
}