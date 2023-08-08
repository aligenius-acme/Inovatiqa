using System;
using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Http.Extensions;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Extensions;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Inovatiqa.Web.Models.Checkout;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Controllers
{
    public class CheckoutController : BasePublicController
    {
        #region Fields

        private readonly IWorkContextService _workContextService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IProductService _productService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICountryService _countryService;
        private readonly IAddressAttributeParserService _addressAttributeParserService;
        private readonly IAddressService _addressService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ILoggerService _loggerService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public CheckoutController(
            IWorkContextService workContextService,
            IShoppingCartService shoppingCartService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IProductService productService,
            ICheckoutModelFactory checkoutModelFactory,
            ICountryService countryService,
            IAddressAttributeParserService addressAttributeParserService,
            IAddressService addressService,
            IOrderProcessingService orderProcessingService,
            IPaymentMethodService paymentMethodService,
            ILoggerService loggerService,
            IShippingService shippingService,
            IRazorViewEngine viewEngine,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IProductModelFactory productModelFactory,
            IStateProvinceService stateProvinceService) : base(viewEngine)
        {
            _workContextService = workContextService;
            _shoppingCartService = shoppingCartService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _productService = productService;
            _checkoutModelFactory = checkoutModelFactory;
            _countryService = countryService;
            _addressAttributeParserService = addressAttributeParserService;
            _addressService = addressService;
            _orderProcessingService = orderProcessingService;
            _paymentMethodService = paymentMethodService;
            _loggerService = loggerService;
            _shippingService = shippingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _productModelFactory = productModelFactory;
            _orderTotalCalculationService = orderTotalCalculationService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        protected virtual bool IsMinimumOrderPlacementIntervalValid(Customer customer)
        {
            var currentCustomer = _workContextService.CurrentCustomer.Id;

            if (InovatiqaDefaults.MinimumOrderPlacementInterval == 0)
                return true;

            var lastOrder = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
                customerId: currentCustomer, pageSize: 1)
                .FirstOrDefault();
            if (lastOrder == null)
                return true;

            var interval = DateTime.UtcNow - lastOrder.CreatedOnUtc;
            return interval.TotalSeconds > InovatiqaDefaults.MinimumOrderPlacementInterval;
        }

        #endregion

        #region Methods (common)

        [IgnoreAntiforgeryToken]
        public virtual IActionResult Completed(int? orderId)
        {
            var customer = _workContextService.CurrentCustomer;

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();


            Order order = null;
            if (orderId.HasValue)
            {
                order = _orderService.GetOrderById(orderId.Value);
            }
            if (order == null)
            {
                order = _orderService.SearchOrders(storeId: InovatiqaDefaults.StoreId,
                customerId: customer.Id, pageSize: 1)
                    .FirstOrDefault();
            }
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
            {
                return RedirectToRoute("Homepage");
            }

            if (InovatiqaDefaults.DisableOrderCompletedPage)
            {
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });
            } 

            var model = _checkoutModelFactory.PrepareCheckoutCompletedModel(order);
            var orders = _orderService.GetOrderItems(model.OrderId);
            foreach (var product in orders)
            {
                var products = _productService.GetProductById(product.ProductId);
                products.StockQuantity = products.StockQuantity - product.Quantity;
                _productService.UpdateProduct(products);
                
            }

            return View(model);
        }

        public virtual IActionResult Index()
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();


            _customerService.ResetCheckoutData(customer, InovatiqaDefaults.StoreId);

            var checkoutAttributesXml = _genericAttributeService.GetAttribute<string>(customer,
                InovatiqaDefaults.CheckoutAttributes, customer.Id, InovatiqaDefaults.StoreId);
            var scWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return RedirectToRoute("ShoppingCart");

            foreach (var sci in cart)
            {
                var product = _productService.GetProductById(sci.ProductId);

                var sciWarnings = _shoppingCartService.GetShoppingCartItemWarnings(customer,
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
                if (sciWarnings.Any())
                    return RedirectToRoute("ShoppingCart");
            }

            if (InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            return RedirectToRoute("CheckoutBillingAddress");
        }

        #endregion

        #region Methods (multistep checkout)

        public virtual IActionResult BillingAddress(IFormCollection form)
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();

            var model = _checkoutModelFactory.PrepareBillingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true);

            if (InovatiqaDefaults.DisableBillingAddressCheckoutStep && model.ExistingAddresses.Any())
            {
                if (model.ExistingAddresses.Any())
                {
                    return SelectBillingAddress(model.ExistingAddresses.First().Id);
                }

                TryValidateModel(model);
                TryValidateModel(model.BillingNewAddress);
                return NewBillingAddress(model, form);
            }

            return View(model);
        }

        [HttpPost, ActionName("BillingAddress")]
        [FormValueRequired("nextstep")]
        public virtual IActionResult NewBillingAddress(CheckoutBillingAddressModel model, IFormCollection form)
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();


            var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
            var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            var newAddress = model.BillingNewAddress;

            if (ModelState.IsValid)
            {
                var address = _addressService.FindAddress(_customerService.GetAddressesByCustomerId(customer.Id).ToList(),
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                    newAddress.Address1, newAddress.Address2, newAddress.City,
                    newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId, customAttributes);

                if (address == null)
                {
                    address = newAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;

                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    _addressService.InsertAddress(address);

                    _customerService.InsertCustomerAddress(customer, address);
                }

                customer.BillingAddressId = address.Id;

                _customerService.UpdateCustomer(customer);

                if (InovatiqaDefaults.ShipToSameAddress && model.ShipToSameAddress && _shoppingCartService.ShoppingCartRequiresShipping(cart))
                {
                    customer.ShippingAddressId = customer.BillingAddressId;
                    _customerService.UpdateCustomer(customer);

                    _genericAttributeService.SaveAttribute<ShippingOption>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedShippingOptionAttribute, null, InovatiqaDefaults.StoreId);
                    _genericAttributeService.SaveAttribute<PickupPoint>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedPickupPointAttribute, null, InovatiqaDefaults.StoreId);

                    return RedirectToRoute("CheckoutShippingMethod");
                }

                return RedirectToRoute("CheckoutShippingAddress");
            }

            model = _checkoutModelFactory.PrepareBillingAddressModel(cart,
                selectedCountryId: newAddress.CountryId,
                overrideAttributesXml: customAttributes);
            return View(model);
        }

        public virtual IActionResult ShippingMethod()
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();

            if (!_shoppingCartService.ShoppingCartRequiresShipping(cart))
            {
                _genericAttributeService.SaveAttribute<ShippingOption>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedShippingOptionAttribute, null, InovatiqaDefaults.StoreId);
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            var model = _checkoutModelFactory.PrepareShippingMethodModel(cart, _customerService.GetCustomerShippingAddress(customer));

            if (InovatiqaDefaults.BypassShippingMethodSelectionIfOnlyOne &&
                model.ShippingMethods.Count == 1)
            {
                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id,
                    InovatiqaDefaults.SelectedShippingOptionAttribute,
                    model.ShippingMethods.First().ShippingOption,
                    InovatiqaDefaults.StoreId);

                return RedirectToRoute("CheckoutPaymentMethod");
            }

            return View(model);
        }

        public virtual IActionResult PaymentMethod()
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();

            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart, false);
            if (!isPaymentWorkflowRequired)
            {
                _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id,
                    InovatiqaDefaults.SelectedPaymentMethodAttribute, null, InovatiqaDefaults.StoreId);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            var filterByCountryId = 0;
            if (InovatiqaDefaults.CountryEnabled)
            {
                filterByCountryId = _customerService.GetCustomerBillingAddress(customer)?.CountryId ?? 0;
            }

            var paymentMethodModel = _checkoutModelFactory.PreparePaymentMethodModel(cart, filterByCountryId);

            if (InovatiqaDefaults.BypassPaymentMethodSelectionIfOnlyOne &&
                paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
            {
                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id,
                    InovatiqaDefaults.SelectedPaymentMethodAttribute,
                    paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
                    InovatiqaDefaults.StoreId);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            return View(paymentMethodModel);
        }

        public virtual IActionResult PaymentInfo()
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();

            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(customer,
                InovatiqaDefaults.SelectedPaymentMethodAttribute, InovatiqaDefaults.StoreId);

            var model = _checkoutModelFactory.PreparePaymentInfoModel(paymentMethodSystemName);
            return View(model);
        }

        public virtual IActionResult Confirm()
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();

            var model = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
            return View(model);
        }

        public virtual IActionResult SelectBillingAddress(int addressId, bool shipToSameAddress = false)
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;

            var address = _customerService.GetCustomerAddress(customer.Id, addressId);

            if (address == null)
                return RedirectToRoute("CheckoutBillingAddress");

            customer.BillingAddressId = address.Id;
            _customerService.UpdateCustomer(customer);

            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            var shippingAllowed = InovatiqaDefaults.CountryEnabled ? _countryService.GetCountryByAddress(address)?.AllowsShipping ?? false : true;
            if (InovatiqaDefaults.ShipToSameAddress && shipToSameAddress && _shoppingCartService.ShoppingCartRequiresShipping(cart) && shippingAllowed)
            {
                customer.ShippingAddressId = customer.BillingAddressId;
                _customerService.UpdateCustomer(customer);

                _genericAttributeService.SaveAttribute<ShippingOption>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedShippingOptionAttribute, null, InovatiqaDefaults.StoreId);
                _genericAttributeService.SaveAttribute<PickupPoint>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedPickupPointAttribute, null, InovatiqaDefaults.StoreId);

                return RedirectToRoute("CheckoutShippingMethod");
            }

            return RedirectToRoute("CheckoutShippingAddress");
        }

        #endregion

        #region Methods (one page checkout)

        protected virtual JsonResult OpcLoadStepAfterShippingAddress(IList<ShoppingCartItem> cart, string addNewAddress = "", string addNewAddressId = "")
        {
            var customer = _workContextService.CurrentCustomer;
            var shippingMethodModel = _checkoutModelFactory.PrepareShippingMethodModel(cart, _customerService.GetCustomerShippingAddress(customer));
            if (InovatiqaDefaults.BypassShippingMethodSelectionIfOnlyOne &&
                shippingMethodModel.ShippingMethods.Count == 1)
            {
                _genericAttributeService.SaveAttribute(customer.GetType().Name,
                    customer.Id,
                    InovatiqaDefaults.SelectedShippingOptionAttribute,
                    shippingMethodModel.ShippingMethods.First().ShippingOption,
                    InovatiqaDefaults.StoreId);

                return OpcLoadStepAfterShippingMethod(cart, addNewAddress, addNewAddressId);
            }

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    newAddress = addNewAddress,
                    newAddressId = addNewAddressId,
                    name = "shipping-method",
                    html = RenderPartialViewToString("OpcShippingMethods", shippingMethodModel)
                },
                goto_section = "shipping_method"
            });
        }

        protected virtual JsonResult OpcLoadStepAfterShippingMethod(IList<ShoppingCartItem> cart, string addNewAddress = "", string addNewAddressId = "")
        {
            var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart, false);
            var customer = _workContextService.CurrentCustomer;
            if (isPaymentWorkflowRequired)
            {
                var filterByCountryId = 0;
                if (InovatiqaDefaults.CountryEnabled)
                {
                    filterByCountryId = _customerService.GetCustomerBillingAddress(customer)?.CountryId ?? 0;
                }

                var paymentMethodModel = _checkoutModelFactory.PreparePaymentMethodModel(cart, filterByCountryId);

                //if (InovatiqaDefaults.BypassPaymentMethodSelectionIfOnlyOne &&
                //    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                //{
                //    return OpcLoadStepAfterPaymentMethod(cart);
                //}

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        newAddress = addNewAddress,
                        newAddressId = addNewAddressId,
                        name = "payment-method",
                        html = RenderPartialViewToString("OpcPaymentMethods", paymentMethodModel)
                    },
                    goto_section = "payment_method"
                });
            }

            _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id,
                InovatiqaDefaults.SelectedPaymentMethodAttribute, null, InovatiqaDefaults.StoreId);

            var confirmOrderModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    newAddress = addNewAddress,
                    newAddressId = addNewAddressId,
                    name = "confirm-order",
                    html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                },
                goto_section = "confirm_order"
            });
        }

        protected virtual JsonResult OpcLoadStepAfterPaymentMethod(string paymentMethod, IList<ShoppingCartItem> cart)
        {
            var paymenInfoModel = _checkoutModelFactory.PreparePaymentInfoModel(paymentMethod);
            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "payment-info",
                    html = RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                },
                goto_section = "payment_info"
            });
        }

        public virtual IActionResult OnePageCheckout( string message)
        {
            if (InovatiqaDefaults.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = _workContextService.CurrentCustomer;
            if (customer.ParentId != null && customer.ParentId != 0)
            {
                if (!_customerService.IsInCustomerRole(customer, "Subaccount_GTCC"))
                {
                    return RedirectToAction("NotAllowed", "Common");
                }
            }
            var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (!InovatiqaDefaults.OnePageCheckoutEnabled)
                return RedirectToRoute("Checkout");

            if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                return Challenge();

            var model = _checkoutModelFactory.PrepareOnePageCheckoutModel(cart);
            model.IsB2BAndPOCustomer = _customerService.IsB2B(customer) && _customerService.IsPO(customer);
            return View(model);
        }

        [IgnoreAntiforgeryToken]
        public virtual IActionResult OpcSaveBilling(CheckoutBillingAddressModel model, IFormCollection form)
        {
            var customer = _workContextService.CurrentCustomer;
            string addNewAddressInBillingAddress = "";
            string addNewAddressInBillingAddressId = "";
            try
            {
                if (InovatiqaDefaults.CheckoutDisabled)
                    throw new Exception("Sorry, checkout process is temporary disabled");

                var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!InovatiqaDefaults.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                int.TryParse(form["billing_address_id"], out var billingAddressId);

                if (billingAddressId > 0)
                {
                    var address = _customerService.GetCustomerAddress(customer.Id, billingAddressId)
                        ?? throw new Exception("Address can''t be loaded");

                    customer.BillingAddressId = address.Id;
                    _customerService.UpdateCustomer(customer);
                }
                else
                {
                    var newAddress = model.BillingNewAddress;

                    var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
                    var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    if (!ModelState.IsValid)
                    {
                        var billingAddressModel = _checkoutModelFactory.PrepareBillingAddressModel(cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        billingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "billing",
                                html = RenderPartialViewToString("OpcBillingAddress", billingAddressModel)
                            },
                            wrong_billing_address = true,
                        });
                    }

                    var address = _addressService.FindAddress(_customerService.GetAddressesByCustomerId(customer.Id).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        if (address.CountryId == 0)
                            address.CountryId = null;

                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;

                        _addressService.InsertAddress(address);

                        _customerService.InsertCustomerAddress(customer, address);
                    }

                    customer.BillingAddressId = address.Id;

                    _customerService.UpdateCustomer(customer);
                    addNewAddressInBillingAddress = customer.BillingAddress.FirstName + " " + customer.BillingAddress.LastName + ", " + customer.BillingAddress.Address1 + ", " + customer.BillingAddress.City + ", " + _stateProvinceService.GetStateProvinceById(Convert.ToInt32(customer.BillingAddress.StateProvinceId))?.Name + ", " + customer.BillingAddress.ZipPostalCode + ", " + _countryService.GetCountryById(Convert.ToInt32(customer.BillingAddress.CountryId))?.Name;
                    addNewAddressInBillingAddressId = Convert.ToString(customer.BillingAddressId);
                    
                }
                if (_shoppingCartService.ShoppingCartRequiresShipping(cart))
                {
                    var address = _customerService.GetCustomerBillingAddress(customer);

                    var shippingAllowed = InovatiqaDefaults.CountryEnabled ? _countryService.GetCountryByAddress(address)?.AllowsShipping ?? false : true;
                    if (InovatiqaDefaults.ShipToSameAddress && model.ShipToSameAddress && shippingAllowed)
                    {
                        customer.ShippingAddressId = address.Id;
                        _customerService.UpdateCustomer(customer);
                        _genericAttributeService.SaveAttribute<ShippingOption>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedShippingOptionAttribute, null, InovatiqaDefaults.StoreId);
                        _genericAttributeService.SaveAttribute<PickupPoint>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedPickupPointAttribute, null, InovatiqaDefaults.StoreId);
                        return OpcLoadStepAfterShippingAddress(cart, addNewAddressInBillingAddress, addNewAddressInBillingAddressId);
                    }

                    var shippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(cart, prePopulateNewAddressWithCustomerFields: true);

                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            newAddress = addNewAddressInBillingAddress,
                            newAddressId = addNewAddressInBillingAddressId,
                            name = "shipping",
                            html = RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                        },
                        goto_section = "shipping"
                    });
                }

                _genericAttributeService.SaveAttribute<ShippingOption>(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedShippingOptionAttribute, null, InovatiqaDefaults.StoreId);

                return OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                _loggerService.Warning(exc.Message, exc, customer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public virtual IActionResult OpcSaveShipping(CheckoutShippingAddressModel model, IFormCollection form)
        {
            var customer = _workContextService.CurrentCustomer;
            string addNewAddressInShippingAddress = "";
            string addNewAddressInShippingAddressId = "";
            try
            {
                if (InovatiqaDefaults.CheckoutDisabled)
                    throw new Exception("Sorry, checkout process is temporary disabled");

                var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!InovatiqaDefaults.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!_shoppingCartService.ShoppingCartRequiresShipping(cart))
                    throw new Exception("Shipping is not required");

                int.TryParse(form["shipping_address_id"], out var shippingAddressId);

                if (shippingAddressId > 0)
                {
                    var address = _customerService.GetCustomerAddress(customer.Id, shippingAddressId)
                        ?? throw new Exception("Address can''t be loaded");

                    customer.ShippingAddressId = address.Id;
                    _customerService.UpdateCustomer(customer);
                }
                else
                {
                    var newAddress = model.ShippingNewAddress;

                    var customAttributes = _addressAttributeParserService.ParseCustomAddressAttributes(form);
                    var customAttributeWarnings = _addressAttributeParserService.GetAttributeWarnings(customAttributes);
                    foreach (var error in customAttributeWarnings)
                    {
                        ModelState.AddModelError("", error);
                    }

                    if (!ModelState.IsValid)
                    {
                        var shippingAddressModel = _checkoutModelFactory.PrepareShippingAddressModel(cart,
                            selectedCountryId: newAddress.CountryId,
                            overrideAttributesXml: customAttributes);
                        shippingAddressModel.NewAddressPreselected = true;
                        return Json(new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "shipping",
                                html = RenderPartialViewToString("OpcShippingAddress", shippingAddressModel)
                            }
                        });
                    }

                    var address = _addressService.FindAddress(_customerService.GetAddressesByCustomerId(customer.Id).ToList(),
                        newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                        newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                        newAddress.Address1, newAddress.Address2, newAddress.City,
                        newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                        newAddress.CountryId, customAttributes);

                    if (address == null)
                    {
                        address = newAddress.ToEntity();
                        address.CustomAttributes = customAttributes;
                        address.CreatedOnUtc = DateTime.UtcNow;

                        _addressService.InsertAddress(address);

                        _customerService.InsertCustomerAddress(customer, address);
                    }

                    customer.ShippingAddressId = address.Id;

                    _customerService.UpdateCustomer(customer);
                    addNewAddressInShippingAddress = customer.ShippingAddress.FirstName + " " + customer.ShippingAddress.LastName + ", " + customer.ShippingAddress.Address1 + ", " + customer.ShippingAddress.City + ", " + _stateProvinceService.GetStateProvinceById(Convert.ToInt32(customer.ShippingAddress.StateProvinceId))?.Name + ", " + customer.ShippingAddress.ZipPostalCode + ", " + _countryService.GetCountryById(Convert.ToInt32(customer.ShippingAddress.CountryId))?.Name;
                    addNewAddressInShippingAddressId = customer.ShippingAddressId.ToString();
                }
                return OpcLoadStepAfterShippingAddress(cart, addNewAddressInShippingAddress, addNewAddressInShippingAddressId);
            }
            catch (Exception exc)
            {
                _loggerService.Warning(exc.Message, exc, customer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual IActionResult OpcSaveShippingMethod(string shippingoption, IFormCollection form)
        {
            var customer = _workContextService.CurrentCustomer;
            try
            {
                if (InovatiqaDefaults.CheckoutDisabled)
                    throw new Exception("Sorry, checkout process is temporary disabled");

                var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!InovatiqaDefaults.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!_shoppingCartService.ShoppingCartRequiresShipping(cart))
                    throw new Exception("Shipping is not required");

                if (string.IsNullOrEmpty(shippingoption))
                    throw new Exception("Selected shipping method can't be parsed");
                var splittedOption = shippingoption.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);
                if (splittedOption.Length != 2)
                    throw new Exception("Selected shipping method can't be parsed");
                var selectedName = splittedOption[0];
                var shippingRateComputationMethodSystemName = splittedOption[1];

                var shippingOptions = _genericAttributeService.GetAttribute<List<ShippingOption>>(customer,
                    InovatiqaDefaults.OfferedShippingOptionsAttribute, InovatiqaDefaults.StoreId);
                if (shippingOptions == null || !shippingOptions.Any())
                {
                    shippingOptions = _shippingService.GetShippingOptions(cart, _customerService.GetCustomerShippingAddress(customer),
                        customer, shippingRateComputationMethodSystemName, InovatiqaDefaults.StoreId).ShippingOptions.ToList();
                }
                else
                {
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.SelectedShippingOptionAttribute, shippingOption, InovatiqaDefaults.StoreId);

                return OpcLoadStepAfterShippingMethod(cart);
            }
            catch (Exception exc)
            {
                _loggerService.Warning(exc.Message, exc, customer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual IActionResult OpcSavePaymentMethod(string paymentMethod, CheckoutPaymentMethodModel model)
        {
            var customer = _workContextService.CurrentCustomer;
            try
            {
                if (InovatiqaDefaults.CheckoutDisabled)
                    throw new Exception("Sorry, checkout process is temporary disabled");

                var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!InovatiqaDefaults.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (string.IsNullOrEmpty(paymentMethod))
                    throw new Exception("Selected payment method can't be parsed");

                var isPaymentWorkflowRequired = _orderProcessingService.IsPaymentWorkflowRequired(cart);
                //var isPaymentWorkflowRequired = false;
                if (!isPaymentWorkflowRequired)
                {
                    _genericAttributeService.SaveAttribute<string>(customer.GetType().Name, customer.Id,
                        InovatiqaDefaults.SelectedPaymentMethodAttribute, null, InovatiqaDefaults.StoreId);

                    var confirmOrderModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id,
                    InovatiqaDefaults.SelectedPaymentMethodAttribute, paymentMethod, InovatiqaDefaults.StoreId);

                return OpcLoadStepAfterPaymentMethod(paymentMethod, cart);
            }
            catch (Exception exc)
            {
                _loggerService.Warning(exc.Message, exc, customer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual IActionResult OpcSavePaymentInfo(IFormCollection form)
        {
            var customer = _workContextService.CurrentCustomer;
            try
            {
                if (InovatiqaDefaults.CheckoutDisabled)
                    throw new Exception("Sorry, checkout process is temporary disabled");

                var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!InovatiqaDefaults.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                var paymentMethodSystemName = _genericAttributeService.GetAttribute<string>(customer,
                InovatiqaDefaults.SelectedPaymentMethodAttribute, customer.Id, InovatiqaDefaults.StoreId);

                var warnings = _paymentMethodService.ValidatePaymentForm(form);
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
                if (ModelState.IsValid)
                {
                    ProcessPaymentRequest paymentInfo = null;
                    if(paymentMethodSystemName == InovatiqaDefaults.PurchaseOrderPaymentName)
                        paymentInfo = _paymentMethodService.GetPaymentInfoPO(form);
                    else
                        paymentInfo = _paymentMethodService.GetPaymentInfo(form);
                    _paymentService.GenerateOrderGuid(paymentInfo);

                    HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

                    var confirmOrderModel = _checkoutModelFactory.PrepareConfirmOrderModel(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                var paymenInfoModel = _checkoutModelFactory.PreparePaymentInfoModel(paymentMethodSystemName);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-info",
                        html = RenderPartialViewToString("OpcPaymentInfo", paymenInfoModel)
                    }
                });
            }
            catch (Exception exc)
            {
                _loggerService.Warning(exc.Message, exc, customer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        [IgnoreAntiforgeryToken]
        public virtual IActionResult OpcConfirmOrder( string message, int downloadId)
        {
            var customer = _workContextService.CurrentCustomer;
            try
            {
                if (InovatiqaDefaults.CheckoutDisabled)
                    throw new Exception("Sorry, checkout process is temporary disabled");

                var cart = _shoppingCartService.GetShoppingCart(customer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (customer.PaymentModeId == (int)PaymentModes.PaymentTerms)
                {
                    if (customer.CreditLimit != null)
                    {
                        var customerUnPaidOrders = _customerService.GetCustomerUnPaidOrders(customer);
                        var unPaidOrdersTotal = customerUnPaidOrders.Sum(x => x.OrderTotal);
                        var shoppingCartTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart);
                        if (shoppingCartTotal != null)
                            unPaidOrdersTotal += decimal.Parse(shoppingCartTotal.ToString());
                        if (unPaidOrdersTotal >= customer.CreditLimit)
                            throw new Exception("Credit limit reached.");
                    }
                }



                if (!InovatiqaDefaults.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (_customerService.IsGuest(customer) && !InovatiqaDefaults.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!IsMinimumOrderPlacementIntervalValid(customer))
                    throw new Exception("Please wait several seconds before placing a new order (already placed another order several seconds ago).");

                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    if (_orderProcessingService.IsPaymentWorkflowRequired(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = InovatiqaDefaults.StoreId;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = _genericAttributeService.GetAttribute<string>(customer,
                InovatiqaDefaults.SelectedPaymentMethodAttribute, customer.Id, InovatiqaDefaults.StoreId);
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = _orderProcessingService.PlaceOrder(processPaymentRequest);
                
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    if (_paymentMethodService.PaymentMethodType == InovatiqaDefaults.RedirectionType)
                    {
                        return Json(new
                        {
                            redirect = $"{InovatiqaDefaults.StoreUrl}checkout/OpcCompleteRedirectionPayment"
                        });
                    }

                    _paymentService.PostProcessPayment(postProcessPaymentRequest);
                    customer.CanPurchaseCart = false;
                    _customerService.UpdateCustomer(customer);
                    return Json(new { success = 1 });
                }
                else
                {
                    return Json(new { error = 1, message = placeOrderResult.Errors[0] });
                }

                if (!string.IsNullOrEmpty(message))
                {
                    var orderNote = new OrderNote
                    {
                        OrderId = placeOrderResult.PlacedOrder.Id,
                        DisplayToCustomer = true,
                        Note = message,
                        DownloadId = downloadId,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    _orderService.InsertOrderNote(orderNote);
                }

                var confirmOrderModel = new CheckoutConfirmModel();
                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = RenderPartialViewToString("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                });
            }
            catch (Exception exc)
            {
                _loggerService.Warning(exc.Message, exc, _workContextService.CurrentCustomer);
                return Json(new { error = 1, message = exc.Message });
            }
        }

        #endregion

        #region Quick add product
        public virtual IActionResult LoadQuickProductView(int productId ,decimal unitprice, int shoppingCartTypeId = 0, bool editing = false)
        {
            var product = _productService.GetProductBySku(Convert.ToString(productId));
            

            var cart = new List<ShoppingCartItem>();
            var prod = new ShoppingCartItem();
            var model = new ProductDetailsModel();
            if (editing)
            {
                cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, shoppingCartTypeId).ToList();
                prod = cart.Where(sci => sci.Id == productId).First();
                product = _productService.GetProductById(prod.ProductId);
                model = _productModelFactory.PrepareProductDetailsModel(product, prod, false, unitprice, editing);
                
            }
            else
            {
                if (product == null)
                    product = _productService.GetProductById(productId);
                if (product == null || product.Deleted)
                    return InvokeHttp404();
                    
                model = _productModelFactory.PrepareProductDetailsModel(product, null, false, 0 ,editing);

            }

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "quick-productview",
                    html = RenderPartialViewToString("_QuickProductView", model)
                },
                goto_section = "quick-productview"
            });
        }
        //public virtual IActionResult LoadQuickProduct(int productId)
        //{
        //    var product = _productService.GetProductById(productId);
        //    if (product == null || product.Deleted)
        //        return InvokeHttp404();


        //    var model = _productModelFactory.PrepareProductDetailsModel(product, null, false);
        //    return Json(new
        //    {
        //        update_section = new UpdateSectionJsonModel
        //        {
        //            name = "quick-product",
        //            html = RenderPartialViewToString("_QuickProduct", model)
        //        },
        //        goto_section = "quick-product"
        //    });
        //}
        #endregion
    }
}
