using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Models.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Components
{
    public class PaymentSquareViewComponent : ViewComponent
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContextService _workContextService;
        private readonly ISquarePaymentManagerService _squarePaymentManagerService;

        #endregion

        #region Ctor

        public PaymentSquareViewComponent(ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IWorkContextService workContextService,
            ISquarePaymentManagerService squarePaymentManagerService)
        {
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _workContextService = workContextService;
            _squarePaymentManagerService = squarePaymentManagerService;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var customer = _workContextService.CurrentCustomer;

            var model = new PaymentInfoModel
            {
                IsGuest = _customerService.IsGuest(customer),

                PostalCode = _customerService.GetCustomerBillingAddress(customer)?.ZipPostalCode
                    ?? _customerService.GetCustomerShippingAddress(customer)?.ZipPostalCode
            };

            var storeId = InovatiqaDefaults.StoreId;
            var customerId = _genericAttributeService
                .GetAttribute<string>(customer, InovatiqaDefaults.CustomerIdAttribute, customer.Id) ?? string.Empty;
            
            var paymentCustomer = _squarePaymentManagerService.GetCustomer(customerId, storeId);
            if (paymentCustomer?.Cards != null)
            {
                var cardNumberMask = "*{0}";
                model.StoredCards = paymentCustomer.Cards
                    .Select(card => new SelectListItem { Text = string.Format(cardNumberMask, card.Last4), Value = card.Id })
                    .ToList();
            }

            if (model.StoredCards.Any())
            {
                var selectCardText = "Select a card";
                model.StoredCards.Insert(0, new SelectListItem { Text = selectCardText, Value = Guid.Empty.ToString() });
            }

            if (InovatiqaDefaults.Use3ds)
            {
                var cart = _shoppingCartService.GetShoppingCart(customer,
                    (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);
                model.OrderTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart, false, false) ?? decimal.Zero;

                var currency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);
                model.Currency = currency?.CurrencyCode;

                var billingAddress = _customerService.GetCustomerBillingAddress(customer);
                var country = _countryService.GetCountryByAddress(billingAddress);
                var stateProvince = _stateProvinceService.GetStateProvinceByAddress(billingAddress);

                model.BillingFirstName = billingAddress?.FirstName;
                model.BillingLastName = billingAddress?.LastName;
                model.BillingEmail = billingAddress?.Email;
                model.BillingCountry = country?.TwoLetterIsoCode;
                model.BillingState = stateProvince?.Abbreviation;
                model.BillingCity = billingAddress?.City;
                model.BillingPostalCode = billingAddress?.ZipPostalCode;
            }

            return View("~/Views/Checkout/PaymentInfo.cshtml", model);
        }

        #endregion
    }
}