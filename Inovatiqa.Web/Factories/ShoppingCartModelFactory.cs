using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Media;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Inovatiqa.Web.Models.ShoppingCart;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Vendors.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Services.Payments;
using Inovatiqa.Core.Http.Extensions;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using System.Globalization;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Database.Interfaces;
using System.Text.RegularExpressions;

namespace Inovatiqa.Web.Factories
{
    public partial class ShoppingCartModelFactory : IShoppingCartModelFactory
    {
        #region Fields

        private readonly IWorkContextService _workContextService;
        private readonly ICustomerService _customerService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductAttributeFormatterService _productAttributeFormatter;
        private readonly IPictureService _pictureService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICheckoutAttributeParserService _checkoutAttributeParserService;
        private readonly IVendorService _vendorService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelperService _dateTimeHelperServiceService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IAddressService _addressService;
        private readonly IRepository<ProductCategoryMapping> _productCategoryRepository;
        private readonly IRepository<Category> _categoryRepository;

        #endregion

        #region Ctor

        public ShoppingCartModelFactory(IWorkContextService workContextService,
            ICustomerService customerService,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPriceFormatter priceFormatter,
            ICheckoutAttributeService checkoutAttributeService,
            IOrderProcessingService orderProcessingService,
            IProductService productService,
            IUrlRecordService urlRecordService,
            IProductAttributeFormatterService productAttributeFormatter,
            IPictureService pictureService,
            IGenericAttributeService genericAttributeService,
            ICheckoutAttributeParserService checkoutAttributeParserService,
            IVendorService vendorService,
            IAddressModelFactory addressModelFactory,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IHttpContextAccessor httpContextAccessor,
            IPaymentService paymentService,
            IShippingService shippingService,
            ICurrencyService currencyService,
            IDateTimeHelperService dateTimeHelperServiceService,
            IProductModelFactory productModelFactory,
            IManufacturerService manufacturerService,
            IAddressService addressService,
            IRepository<ProductCategoryMapping> productCategoryRepository,
            IRepository<Category> categoryRespository)
        {
            _workContextService = workContextService;
            _customerService = customerService;
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceFormatter = priceFormatter;
            _checkoutAttributeService = checkoutAttributeService;
            _orderProcessingService = orderProcessingService;
            _productService = productService;
            _urlRecordService = urlRecordService;
            _productAttributeFormatter = productAttributeFormatter;
            _pictureService = pictureService;
            _genericAttributeService = genericAttributeService;
            _checkoutAttributeParserService = checkoutAttributeParserService;
            _vendorService = vendorService;
            _addressModelFactory = addressModelFactory;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _httpContextAccessor = httpContextAccessor;
            _paymentService = paymentService;
            _shippingService = shippingService;
            _currencyService = currencyService;
            _dateTimeHelperServiceService = dateTimeHelperServiceService;
            _productModelFactory = productModelFactory;
            _manufacturerService = manufacturerService;
            _addressService = addressService;
            _productCategoryRepository = productCategoryRepository;
            _categoryRepository = categoryRespository;
        }

        #endregion

        #region Utilities
        protected virtual IList<ManufacturerBriefInfoModel> PrepareProductManufacturerModels(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = _manufacturerService.GetProductManufacturersByProductId(product.Id)
                .Select(pm =>
                {
                    var manufacturer = _manufacturerService.GetManufacturerById(pm.ManufacturerId);
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.Name,
                        SeName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId)
                    };

                    return modelMan;
                }).ToList();

            return model;
        }

        protected virtual ShoppingCartModel.ShoppingCartItemModel PrepareShoppingCartItemModel(IList<ShoppingCartItem> cart, ShoppingCartItem sci)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            var product = _productService.GetProductById(sci.ProductId);

            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = _productService.FormatSku(product, sci.AttributesXml),
                VendorName = InovatiqaDefaults.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorByProductId(product.Id)?.Name : string.Empty,
                ProductId = sci.ProductId,
                ProductName = product.Name,
                ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                Quantity = sci.Quantity,
                AttributeInfo = _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml),
                ManufacturerPartNumber = sci.Product.ManufacturerPartNumber,
                ProductManufacturers = PrepareProductManufacturerModels(product)
            };
            if (!string.IsNullOrEmpty(cartItemModel.AttributeInfo))
            {
                int startIndex = cartItemModel.AttributeInfo.IndexOf("UOM:") + 4;
                int endIndex = cartItemModel.AttributeInfo.Length;
                if (cartItemModel.AttributeInfo.IndexOf("[") != -1)
                    endIndex = cartItemModel.AttributeInfo.IndexOf("[");
                else if (cartItemModel.AttributeInfo.IndexOf("<br />") != -1)
                    endIndex = cartItemModel.AttributeInfo.IndexOf("<br />");
                cartItemModel.UOM = cartItemModel.AttributeInfo.Substring(startIndex, endIndex - startIndex).Trim();
            }
            cartItemModel.AllowItemEditing = InovatiqaDefaults.AllowCartItemEditing &&
                                             product.ProductTypeId == InovatiqaDefaults.SimpleProduct &&
                                             (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              product.IsGiftCard) &&
                                             product.VisibleIndividually;

            cartItemModel.DisableRemoval = _shoppingCartService.GetProductsRequiringProduct(cart, product).Any();

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });
            }

            if (product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format("[Auto-ship, Every {0} {1}]",
                        product.RecurringCycleLength, product.RecurringCyclePeriodId);


            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(_shoppingCartService.GetUnitPrice(sci));


            cartItemModel.SubTotal = _priceFormatter.FormatPrice(_shoppingCartService.GetSubTotal(sci));


            if (InovatiqaDefaults.ShowProductImagesOnShoppingCart)
            {
                cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                    InovatiqaDefaults.CartThumbPictureSize, true, cartItemModel.ProductName);
            }

            var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                _workContextService.CurrentCustomer,
                sci.ShoppingCartTypeId,
                product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }

        protected virtual ShoppingCartModel.OrderReviewDataModel PrepareOrderReviewDataModel(IList<ShoppingCartItem> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var model = new ShoppingCartModel.OrderReviewDataModel
            {
                Display = true
            };

            var billingAddress = _customerService.GetCustomerBillingAddress(_workContextService.CurrentCustomer);
            if (billingAddress != null)
            {
                _addressModelFactory.PrepareAddressModel(model.BillingAddress,
                        address: billingAddress,
                        excludeProperties: false);
            }

            if (_shoppingCartService.ShoppingCartRequiresShipping(cart))
            {
                model.IsShippable = true;

                var currentCustomer = _workContextService.CurrentCustomer;

                var pickupPoint = _genericAttributeService.GetAttribute<PickupPoint>(currentCustomer,
                    InovatiqaDefaults.SelectedPickupPointAttribute, currentCustomer.Id, InovatiqaDefaults.StoreId);
                model.SelectedPickupInStore = InovatiqaDefaults.AllowPickupInStore && pickupPoint != null;
                if (!model.SelectedPickupInStore)
                {
                    if (_customerService.GetCustomerShippingAddress(_workContextService.CurrentCustomer) is Address address)
                    {
                        _addressModelFactory.PrepareAddressModel(model.ShippingAddress,
                            address: address,
                            excludeProperties: false);
                    }
                }
                else
                {
                    var country = _countryService.GetCountryByTwoLetterIsoCode(pickupPoint.CountryCode);
                    var state = _stateProvinceService.GetStateProvinceByAbbreviation(pickupPoint.StateAbbreviation, country?.Id);

                    model.PickupAddress = new AddressModel
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        County = pickupPoint.County,
                        CountryName = country?.Name ?? string.Empty,
                        StateProvinceName = state?.Name ?? string.Empty,
                        ZipPostalCode = pickupPoint.ZipPostalCode
                    };
                }

                var shippingOption = _genericAttributeService.GetAttribute<ShippingOption>(_workContextService.CurrentCustomer,
                    InovatiqaDefaults.SelectedShippingOptionAttribute, currentCustomer.Id, InovatiqaDefaults.StoreId);
                if (shippingOption != null)
                    model.ShippingMethod = shippingOption.Name;
            }

            ////////var selectedPaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(_workContextService.CurrentCustomer, InovatiqaDefaults.SelectedPaymentMethodAttribute, InovatiqaDefaults.StoreId);
            ////////var paymentMethod = _paymentPluginManager
            ////////    .LoadPluginBySystemName(selectedPaymentMethodSystemName, _workContext.CurrentCustomer, _storeContext.CurrentStore.Id);
            ////////model.PaymentMethod = paymentMethod != null
            ////////    ? _localizationService.GetLocalizedFriendlyName(paymentMethod, _workContext.WorkingLanguage.Id)
            ////////    : string.Empty;

            var processPaymentRequest = _httpContextAccessor.HttpContext?.Session?.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (processPaymentRequest != null)
                model.CustomValues = processPaymentRequest.CustomValues;

            return model;
        }

        protected virtual WishlistModel.ShoppingCartItemModel PrepareWishlistItemModel(ShoppingCartItem sci)
        {
            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            var product = _productService.GetProductById(sci.ProductId);

            var cartItemModel = new WishlistModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = _productService.FormatSku(product, sci.AttributesXml),
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                Quantity = sci.Quantity,
                AttributeInfo = _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml),
                VendorName = InovatiqaDefaults.ShowVendorOnOrderDetailsPage ? _vendorService.GetVendorByProductId(product.Id)?.Name : string.Empty,
                ManufacturerPartNumber = sci.Product.ManufacturerPartNumber,
                ProductManufacturers = PrepareProductManufacturerModels(product),
                ProductAttributesAndValues = _productModelFactory.PrepareProductAttributeModels(product, null)
            };
            if (!string.IsNullOrEmpty(cartItemModel.AttributeInfo))
            {
                int startIndex = cartItemModel.AttributeInfo.IndexOf("UOM:") + 4;
                int endIndex = cartItemModel.AttributeInfo.Length;
                if (cartItemModel.AttributeInfo.IndexOf("[") != -1)
                    endIndex = cartItemModel.AttributeInfo.IndexOf("[");
                else if (cartItemModel.AttributeInfo.IndexOf("<br />") != -1)
                    endIndex = cartItemModel.AttributeInfo.IndexOf("<br />");
                cartItemModel.UOM = cartItemModel.AttributeInfo.Substring(startIndex, endIndex - startIndex).Trim();
            }

            cartItemModel.AllowItemEditing = InovatiqaDefaults.AllowCartItemEditing &&
                                             product.ProductTypeId == InovatiqaDefaults.SimpleProduct &&
                                             (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              product.IsGiftCard) &&
                                             product.VisibleIndividually;

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });
            }

            var shoppingCartUnitPriceWithDiscountBase = _shoppingCartService.GetUnitPrice(sci);
            var shoppingCartUnitPriceWithDiscount = shoppingCartUnitPriceWithDiscountBase;
            cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);

            var shoppingCartItemSubTotalWithDiscountBase = _shoppingCartService.GetSubTotal(sci, true, out var shoppingCartItemDiscountBase, out _, out var maximumDiscountQty);
            var shoppingCartItemSubTotalWithDiscount = shoppingCartItemSubTotalWithDiscountBase;
            cartItemModel.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
            cartItemModel.MaximumDiscountedQty = maximumDiscountQty;


            if (InovatiqaDefaults.ShowProductImagesOnWishlist)
            {
                cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                    InovatiqaDefaults.CartThumbPictureSize, true, cartItemModel.ProductName);
            }

            var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(
                _workContextService.CurrentCustomer,
                sci.ShoppingCartTypeId,
                product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }

        #endregion

        #region Methods

        public virtual EstimateShippingModel PrepareEstimateShippingModel(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var model = new EstimateShippingModel
            {
                Enabled = cart.Any() && _shoppingCartService.ShoppingCartRequiresShipping(cart)
            };
            if (model.Enabled)
            {
                var shippingAddress = _customerService.GetCustomerShippingAddress(_workContextService.CurrentCustomer);
                if (shippingAddress == null)
                {
                    shippingAddress = _customerService.GetAddressesByCustomerId(_workContextService.CurrentCustomer.Id)
                    .FirstOrDefault(a => a.CountryId == null);
                }

                var defaultEstimateCountryId = (setEstimateShippingDefaultAddress && shippingAddress != null)
                    ? shippingAddress.CountryId
                    : model.CountryId;
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = "Select country",
                    Value = "0"
                });

                foreach (var c in _countryService.GetAllCountriesForShipping(InovatiqaDefaults.LanguageId))
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = c.Name,
                        Value = c.Id.ToString(),
                        Selected = c.Id == defaultEstimateCountryId
                    });

                var defaultEstimateStateId = (setEstimateShippingDefaultAddress && shippingAddress != null)
                    ? shippingAddress.StateProvinceId
                    : model.StateProvinceId;
                var states = defaultEstimateCountryId.HasValue
                    ? _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId.Value, InovatiqaDefaults.LanguageId).ToList()
                    : new List<StateProvince>();
                if (states.Any())
                {
                    foreach (var s in states)
                    {
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = s.Name,
                            Value = s.Id.ToString(),
                            Selected = s.Id == defaultEstimateStateId
                        });
                    }
                }
                else
                {
                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = "Other",
                        Value = "0"
                    });
                }

                if (setEstimateShippingDefaultAddress && shippingAddress != null)
                    model.ZipPostalCode = shippingAddress.ZipPostalCode;
            }

            return model;
        }

        public virtual MiniShoppingCartModel PrepareMiniShoppingCartModel()
        {
            var model = new MiniShoppingCartModel
            {
                ShowProductImages = InovatiqaDefaults.ShowProductImagesInMiniShoppingCart,
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = _customerService.IsGuest(_workContextService.CurrentCustomer),
                AnonymousCheckoutAllowed = InovatiqaDefaults.AnonymousCheckoutAllowed,
            };

            if (_workContextService.CurrentCustomer.HasShoppingCartItems)
            {
                var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);
                model.TotalItems = cart.Count();
                if (cart.Any())
                {
                    //model.TotalProducts = cart.Sum(item => item.Quantity);

                    var subTotalIncludingTax = InovatiqaDefaults.SubTotalIncludingTax;
                    _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax, out var _, out var _, out var subTotalWithoutDiscountBase, out var _);
                    var subtotalBase = subTotalWithoutDiscountBase;
                    var subtotal = subtotalBase;
                    model.SubTotal = _priceFormatter.FormatPrice(subtotal);

                    var requiresShipping = _shoppingCartService.ShoppingCartRequiresShipping(cart);
                    var checkoutAttributesExist = _checkoutAttributeService
                        .GetAllCheckoutAttributes(InovatiqaDefaults.StoreId, !requiresShipping)
                        .Any();

                    var minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);

                    var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();


                    model.DisplayCheckoutButton = minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist;

                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id).ToList())
                    {
                        var product = _productService.GetProductById(sci.ProductId);

                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                        {
                            Id = sci.Id,
                            ProductId = sci.ProductId,
                            ProductName = product.Name,
                            ProductSeName = _urlRecordService.GetActiveSlug(product.Id, InovatiqaDefaults.ProductSlugName, InovatiqaDefaults.LanguageId),
                            Quantity = sci.Quantity,
                            AttributeInfo = _productAttributeFormatter.FormatAttributes(product, sci.AttributesXml)
                        };

                        _shoppingCartService.GetUnitPrice(sci);

                        var shoppingCartUnitPriceWithDiscountBase = _shoppingCartService.GetUnitPrice(sci);
                        var shoppingCartUnitPriceWithDiscount = shoppingCartUnitPriceWithDiscountBase;
                        cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);

                        if (InovatiqaDefaults.ShowProductImagesInMiniShoppingCart)
                        {
                            cartItemModel.Picture = PrepareCartItemPictureModel(sci,
                                InovatiqaDefaults.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                        }

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        public virtual PictureModel PrepareCartItemPictureModel(ShoppingCartItem sci, int pictureSize, bool showDefaultPicture, string productName)
        {
            var product = _productService.GetProductById(sci.ProductId);

            var sciPicture = _pictureService.GetProductPicture(product, sci.AttributesXml);

            return new PictureModel
            {
                ImageUrl = _pictureService.GetPictureUrl(ref sciPicture, pictureSize, showDefaultPicture),
                Title = string.Format("Show details for {0}", productName),
                AlternateText = string.Format("Picture of {0}", productName),
            };
        }

        public virtual ShoppingCartModel PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareAndDisplayOrderReviewData = false)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.OnePageCheckoutEnabled = InovatiqaDefaults.OnePageCheckoutEnabled;

            if (!cart.Any())
            {
                model.CanCustomerPurchase = true;
                return model;
            }

            model.IsEditable = isEditable;
            model.ShowProductImages = InovatiqaDefaults.ShowProductImagesOnShoppingCart;
            model.ShowSku = InovatiqaDefaults.ShowSkuOnProductDetailsPage;
            model.ShowVendorName = InovatiqaDefaults.ShowVendorOnOrderDetailsPage;
            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(_workContextService.CurrentCustomer,
                InovatiqaDefaults.CheckoutAttributes, InovatiqaDefaults.StoreId);
            var minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
            if (!minOrderSubtotalAmountOk)
            {
                var minOrderSubtotalAmount = InovatiqaDefaults.MinOrderSubtotalAmount;
                model.MinOrderSubtotalWarning = string.Format("Minimum order sub-total amount is {0}", _priceFormatter.FormatPrice(minOrderSubtotalAmount));
            }

            model.TermsOfServiceOnShoppingCartPage = InovatiqaDefaults.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = InovatiqaDefaults.TermsOfServiceOnOrderConfirmPage;
            model.TermsOfServicePopup = InovatiqaDefaults.PopupForTermsOfServiceLinks;
            model.DisplayTaxShippingInfo = InovatiqaDefaults.DisplayTaxShippingInfoShoppingCart;

            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            decimal OrderTotal = 0;
            foreach (var sci in cart)
            {
                var cartItemModel = PrepareShoppingCartItemModel(cart, sci);
                model.Items.Add(cartItemModel);
                OrderTotal += ((sci.Quantity) * (_productService.GetProductById(sci.ProductId).Price));
            }

            model.HideCheckoutButton = false;

            if (prepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData = PrepareOrderReviewDataModel(cart);
            }
            model.CanCustomerPurchase = true;
            var customer = _workContextService.CurrentCustomer;
            if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (customer.CanPurchaseCart!=null && Convert.ToBoolean(customer.CanPurchaseCart))
                {
                    model.CanCustomerPurchase = true;
                }
                else if (_customerService.IsInCustomerRole(customer, "Subaccount_RABCO"))
                {
                    model.CanCustomerPurchase = OrderTotal <= customer.MaxOrderWithoutApproval;
                }
            }
            return model;
        }

        public virtual List<CustomerSuspendedCartModel> PrepareCustomerSuspendedCartModel(List<CustomerSuspendedCartModel> model,
            IList<SuspendedCart> cart)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var customer = _workContextService.CurrentCustomer;

            var customerSuspendedCarts = new List<CustomerSuspendedCartModel>();

            foreach (var sci in cart)
            {
                var customerSuspendedCartModel = new CustomerSuspendedCartModel
                {
                    Id = sci.Id,
                    POName = sci.Poname,
                    LastModifiedDate = sci.LastModifiedDateUtc,
                    ShipToCompanyName = sci.ShipToCompanyName,
                    ShipToFirstName = sci.ShipToFirstName,
                    ShipToLastName = sci.ShipToLastName,
                    TotalLines = sci.Lines != null ? int.Parse(sci.Lines.ToString()) : 0
                };
                if (customer.ShippingAddressId != null)
                    customerSuspendedCartModel.ShipToAddressId = _addressService.GetAddressById(int.Parse(customer.ShippingAddressId.ToString())).Id;
                model.Add(customerSuspendedCartModel);
            }

            return model;
        }

        public virtual OrderTotalsModel PrepareOrderTotalsModel(IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new OrderTotalsModel
            {
                IsEditable = isEditable
            };

            if (cart.Any())
            {
                var subTotalIncludingTax = InovatiqaDefaults.SubTotalIncludingTax;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart, subTotalIncludingTax, out var orderSubTotalDiscountAmountBase, out var _, out var subTotalWithoutDiscountBase, out var _);
                var subtotalBase = subTotalWithoutDiscountBase;
                var subtotal = subtotalBase;
                model.SubTotal = _priceFormatter.FormatPrice(subtotal);

                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    var orderSubTotalDiscountAmount = orderSubTotalDiscountAmountBase;
                    model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount);
                }

                model.RequiresShipping = _shoppingCartService.ShoppingCartRequiresShipping(cart);
                if (model.RequiresShipping)
                {
                    var shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        model.Shipping = _priceFormatter.FormatPrice(shoppingCartShippingBase.Value);

                        var shippingOption = _genericAttributeService.GetAttribute<ShippingOption>(_workContextService.CurrentCustomer,
                            InovatiqaDefaults.SelectedShippingOptionAttribute, InovatiqaDefaults.StoreId);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }
                else
                {
                    model.HideShippingTotal = true;
                }

                var currentCustomer = _workContextService.CurrentCustomer;

                var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(currentCustomer, InovatiqaDefaults.SelectedPaymentMethodAttribute, currentCustomer.Id, InovatiqaDefaults.StoreId);
                var paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                var paymentMethodAdditionalFeeWithTaxBase = paymentMethodAdditionalFee;
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPrice(paymentMethodAdditionalFeeWithTaxBase);
                }

                bool displayTax;
                bool displayTaxRates;
                if (InovatiqaDefaults.HideTaxInOrderSummary)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    var shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(cart, out var taxRates);

                    if (shoppingCartTaxBase == 0)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = InovatiqaDefaults.DisplayTaxRates && taxRates.Any();
                        displayTax = !displayTaxRates;

                        model.Tax = _priceFormatter.FormatPrice(shoppingCartTaxBase);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatPrice(tr.Key),
                                Value = _priceFormatter.FormatPrice(tr.Value),
                            });
                        }
                    }
                }

                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                var shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart, out var orderTotalDiscountAmountBase, out var _, out var appliedGiftCards, out var redeemedRewardPoints);
                if (shoppingCartTotalBase.HasValue)
                {
                    model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotalBase.Value);
                }

                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmountBase);
                }
            }

            return model;
        }

        public virtual WishlistModel PrepareWishlistModel(WishlistModel model, IList<ShoppingCartItem> cart, bool isEditable = true, int? wishListId = 0, int categoryFilter = -1)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Name = wishListId != 0 ? _shoppingCartService.GetWishList(wishListId).ListName : "ALL";
            model.EmailWishlistEnabled = InovatiqaDefaults.EmailWishlistEnabled;
            model.IsEditable = isEditable;
            model.DisplayAddToCart = InovatiqaDefaults.EnableShoppingCart;
            model.DisplayTaxShippingInfo = InovatiqaDefaults.DisplayTaxShippingInfoWishlist;

            if (!cart.Any())
            {
                if (_customerService.IsRegistered(_workContextService.CurrentCustomer))
                    model.IsRegistered = true;
                else
                    model.IsRegistered = false;
                return model;
            }

            var customer = _customerService.GetShoppingCartCustomer(cart);

            model.CustomerGuid = customer.CustomerGuid;
            model.CustomerFullname = _customerService.GetCustomerFullName(customer);
            model.ShowProductImages = InovatiqaDefaults.ShowProductImagesOnWishlist;
            model.ShowSku = InovatiqaDefaults.ShowSkuOnProductDetailsPage;

            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, string.Empty, false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            foreach (var sci in cart)
            {
                var cartItemModel = PrepareWishlistItemModel(sci);
                if (categoryFilter > 0) // needs specific Category
                {
                    var allMappings = _productCategoryRepository.Query();
                    var allProducts = allMappings.Where(x => x.CategoryId == categoryFilter);
                    foreach(var product in allProducts)
                        if(product.ProductId == cartItemModel.ProductId)
                            model.Items.Add(cartItemModel);
                }
                else
                    model.Items.Add(cartItemModel);
                // add categories of selected products
                var data = _productCategoryRepository.Query();
                var categories = data.Where(x => x.ProductId == sci.ProductId);
                foreach(var category in categories)
                {
                    model.Categories.Add(new KeyValuePair<int, string>(category.CategoryId, _categoryRepository.GetById(category.CategoryId).Name));
                }
            }

            if (_customerService.IsRegistered(customer))
                model.IsRegistered = true;
            else
                model.IsRegistered = false;

            if (_shoppingCartService.IsWishListShared(customer, wishListId))
                model.IsShared = true;
            else
                model.IsShared = false;

            return model;
        }

        public virtual EstimateShippingResultModel PrepareEstimateShippingResultModel(IList<ShoppingCartItem> cart, int? countryId, int? stateProvinceId, string zipPostalCode, bool cacheShippingOptions)
        {
            var model = new EstimateShippingResultModel();

            if (_shoppingCartService.ShoppingCartRequiresShipping(cart))
            {
                var address = new Address
                {
                    CountryId = countryId,
                    StateProvinceId = stateProvinceId,
                    ZipPostalCode = zipPostalCode,
                };

                var rawShippingOptions = new List<ShippingOption>();

                var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, address, _workContextService.CurrentCustomer, storeId: InovatiqaDefaults.StoreId);
                if (getShippingOptionResponse.Success)
                {
                    if (getShippingOptionResponse.ShippingOptions.Any())
                    {
                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            rawShippingOptions.Add(new ShippingOption()
                            {
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,
                                Rate = shippingOption.Rate,
                                TransitDays = shippingOption.TransitDays,
                                ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName
                            });
                        }
                    }
                    else
                        foreach (var error in getShippingOptionResponse.Errors)
                            model.Warnings.Add(error);
                }

                var pickupPointsNumber = 0;
                if (InovatiqaDefaults.AllowPickupInStore)
                {
                    var pickupPointsResponse = _shippingService.GetPickupPoints(address.Id, _workContextService.CurrentCustomer, storeId: InovatiqaDefaults.StoreId);
                    if (pickupPointsResponse.Success)
                    {
                        if (pickupPointsResponse.PickupPoints.Any())
                        {
                            pickupPointsNumber = pickupPointsResponse.PickupPoints.Count();
                            var pickupPoint = pickupPointsResponse.PickupPoints.OrderBy(p => p.PickupFee).First();

                            rawShippingOptions.Add(new ShippingOption()
                            {
                                Name = "Pickup",
                                Description = "Pick up your items at the store",
                                Rate = pickupPoint.PickupFee,
                                TransitDays = pickupPoint.TransitDays,
                                ShippingRateComputationMethodSystemName = pickupPoint.ProviderSystemName,
                                IsPickupInStore = true
                            });
                        }
                    }
                    else
                        foreach (var error in pickupPointsResponse.Errors)
                            model.Warnings.Add(error);
                }

                ShippingOption selectedShippingOption = null;
                if (cacheShippingOptions)
                {
                    _genericAttributeService.SaveAttribute(_workContextService.CurrentCustomer.GetType().Name,
                                                            _workContextService.CurrentCustomer.Id,
                                                           InovatiqaDefaults.OfferedShippingOptionsAttribute,
                                                           rawShippingOptions,
                                                           InovatiqaDefaults.StoreId);

                    selectedShippingOption = _genericAttributeService.GetAttribute<ShippingOption>(_workContextService.CurrentCustomer,
                            InovatiqaDefaults.SelectedShippingOptionAttribute, InovatiqaDefaults.StoreId);
                }

                if (rawShippingOptions.Any())
                {
                    foreach (var option in rawShippingOptions)
                    {
                        var shippingRate = _orderTotalCalculationService.AdjustShippingRate(option.Rate, cart, out var _, option.IsPickupInStore);
                        var shippingRateString = _priceFormatter.FormatPrice(shippingRate);

                        if (option.IsPickupInStore && pickupPointsNumber > 1)
                            shippingRateString = string.Format("From {0}", shippingRateString);

                        string deliveryDateFormat = null;
                        if (option.TransitDays.HasValue)
                        {
                            var currentCulture = CultureInfo.GetCultureInfo(InovatiqaDefaults.LanguageCulture);
                            var customerDateTime = _dateTimeHelperServiceService.ConvertToUserTime(DateTime.Now);
                            deliveryDateFormat = customerDateTime.AddDays(option.TransitDays.Value).ToString("d", currentCulture);
                        }

                        var selected = false;
                        if (selectedShippingOption != null &&
                        !string.IsNullOrEmpty(option.ShippingRateComputationMethodSystemName) &&
                               option.ShippingRateComputationMethodSystemName.Equals(selectedShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase) &&
                               (!string.IsNullOrEmpty(option.Name) &&
                               option.Name.Equals(selectedShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) ||
                               (option.IsPickupInStore && option.IsPickupInStore == selectedShippingOption.IsPickupInStore))
                               )
                        {
                            selected = true;
                        }

                        model.ShippingOptions.Add(new EstimateShippingResultModel.ShippingOptionModel()
                        {
                            Name = option.Name,
                            ShippingRateComputationMethodSystemName = option.ShippingRateComputationMethodSystemName,
                            Description = option.Description,
                            Price = shippingRateString,
                            Rate = option.Rate,
                            DeliveryDateFormat = deliveryDateFormat,
                            Selected = selected
                        });
                    }

                    if (!model.ShippingOptions.Any(so => so.Selected))
                        model.ShippingOptions.First().Selected = true;
                }
            }

            return model;
        }
        public virtual WishlistModel FilterCategories(WishlistModel model)
        {
            var filteredModel = new WishlistModel();
            return filteredModel;
        }

        public virtual IList<ShoppingCartModel.ShoppingCartItemModel> PrepareSuspendedCartItemsModel(int id)
        {
            var customer = _workContextService.CurrentCustomer;
            var model = new ShoppingCartModel();
            var items = _shoppingCartService.GetSuspendedShoppingCart(customer, id);
            foreach(var item in items)
            {
                var product = _productService.GetProductById(item.ProductId);
                var attributes = _productAttributeFormatter.FormatAttributes(product, item.AttributesXml);
                var manufacturers = _manufacturerService.GetProductManufacturersByProductId(product.Id);
                Regex regex = new Regex("UOM: [A-Za-z]{1,4}(/){0,1}[0-9]{1,4}");
                Match attributes_string = regex.Match(attributes);
                model.Items.Add(new ShoppingCartModel.ShoppingCartItemModel
                {
                    Sku = product.Sku,
                    ProductName = product.Name,
                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                    UnitPrice = _priceFormatter.FormatPrice(product.Price),
                    SubTotal = _priceFormatter.FormatPrice(product.Price * item.Quantity),
                    Quantity = item.Quantity,
                    UOM = attributes_string.Value.Replace("UOM: ", "")
                });
            }
            return model.Items;
        }
        #endregion
    }
}