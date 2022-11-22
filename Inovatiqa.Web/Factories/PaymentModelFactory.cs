using System;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Common;
using Inovatiqa.Web.Models.Payment;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Factories
{
    public partial class PaymentModelFactory : IPaymentModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISquarePaymentManagerService _squarePaymentManagerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContextService _workContextService;
        private readonly IOrderService _orderService;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IPaymentService _paymentService;
        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public PaymentModelFactory(ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ISquarePaymentManagerService squarePaymentManagerService,
            ICountryService countryService,
            IShipmentService shipmentService,
            IWorkContextService workContextService,
            IOrderService orderService,
            ICommonModelFactory commonModelFactory,
            IPriceFormatter priceFormatter,
            ICurrencyService currencyService,
            IPaymentService paymentService,
            IAddressService addressService,
            IStateProvinceService stateProvinceService)
        {
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _squarePaymentManagerService = squarePaymentManagerService;
            _countryService = countryService;
            _shipmentService = shipmentService;
            _stateProvinceService = stateProvinceService;
            _workContextService = workContextService;
            _orderService = orderService;
            _commonModelFactory = commonModelFactory;
            _priceFormatter = priceFormatter;
            _currencyService = currencyService;
            _paymentService = paymentService;
            _addressService = addressService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        public virtual PaymentInfoModel PreparePaymentInfoModel(Customer customer,
            decimal totalPayment,
            decimal amountToPay,
            string invoiceIds,
            string invoiceIdsAmounts,
            string orderIds = "")
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var model = new PaymentInfoModel
            {
                IsGuest = _customerService.IsGuest(customer),

                PostalCode = _customerService.GetCustomerBillingAddress(customer)?.ZipPostalCode
                    ?? _customerService.GetCustomerShippingAddress(customer)?.ZipPostalCode
            };

            model.TotalPayment = totalPayment;
            model.AmountToPay = amountToPay;
            model.InvoiceIds = invoiceIds;
            model.OrderIds = orderIds;
            model.invoiceIdsAmounts = invoiceIdsAmounts;

            var storeId = InovatiqaDefaults.StoreId;
            var customerId = _genericAttributeService
                .GetAttribute<string>(customer, InovatiqaDefaults.CustomerIdAttribute, customer.Id) ?? string.Empty;

            var currency = _currencyService.GetCurrencyById(InovatiqaDefaults.PrimaryStoreCurrencyId);
            model.Currency = currency?.CurrencyCode;

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
                var billingAddress = _customerService.GetCustomerBillingAddress(customer);
                model.BillingFirstName = billingAddress?.FirstName;
                model.BillingLastName = billingAddress?.LastName;
                model.BillingEmail = billingAddress?.Email;
                model.BillingCity = billingAddress?.City;
                model.BillingPostalCode = billingAddress?.ZipPostalCode;


                var country = _countryService.GetCountryByAddress(billingAddress);
                var stateProvince = _stateProvinceService.GetStateProvinceByAddress(billingAddress);
                model.BillingAddress = new AddressModel
                {
                    FirstName = billingAddress?.FirstName,
                    LastName = billingAddress?.LastName,
                    Email = billingAddress?.Email,
                    County = country?.TwoLetterIsoCode,
                    StateProvinceName = stateProvince?.Abbreviation,
                    City = billingAddress?.City,
                    ZipPostalCode = billingAddress?.ZipPostalCode
                };
            }
            return model;
        }

        public virtual PaymentShipmentModel PreparePaymentPortalShipmentListModel()
        {
            var customer = _workContextService.CurrentCustomer;
            var customerBillingAddress = _customerService.GetCustomerBillingAddress(customer);
            var country = _countryService.GetCountryByAddress(customerBillingAddress);
            var stateProvince = _stateProvinceService.GetStateProvinceByAddress(customerBillingAddress);
            var model = new PaymentShipmentModel();

            model.BillingAddress.Id = customerBillingAddress.Id;
            model.BillingAddress.FirstName = customerBillingAddress?.FirstName;
            model.BillingAddress.LastName = customerBillingAddress?.LastName;
            model.BillingAddress.Email = customerBillingAddress?.Email;
            model.BillingAddress.County = country?.TwoLetterIsoCode;
            model.BillingAddress.StateProvinceName = stateProvince?.Abbreviation;
            model.BillingAddress.City = customerBillingAddress?.City;
            model.BillingAddress.ZipPostalCode = customerBillingAddress?.ZipPostalCode;

            decimal? totalOpenAmount = 0.0m;
            decimal? totalInvoicedAmount = 0.0m;
            var customerTotalAmount = 0.0m;

            var pastdueDate = 0.0m;
            var pastdueDate30 = 0.0m;
            var pastdueDate60 = 0.0m;
            var pastdueDate90 = 0.0m;

            var orders = _orderService.GetAllOrdersByCustomer(customer);
            foreach (var order in orders)
            {
                var shipments = _shipmentService.GetShipmentsByOrderId(order.Id);
                var billingAddress = _addressService.GetAddressById(order.BillingAddressId);
                
                foreach (var shipment in shipments)
                {
                    shipment.TotalAmount = shipment.TotalAmount != null ? decimal.Parse(shipment.TotalAmount.ToString()) : 0.0m;
                    shipment.AmountPaid = shipment.AmountPaid != null ? decimal.Parse(shipment.AmountPaid.ToString()) : 0.0m;
                    if (billingAddress.ParentAddressId == customer.BillingAddressId)
                    {
                        var paymentShipmentListModel = new PaymentShipmentListModel()
                        {
                            Id = shipment.Id,
                            CreatedDate = shipment.CreatedOnUtc,
                            ShippedDate = shipment.ShippedDateUtc,
                            TrackingNumber = shipment.TrackingNumber,
                            PaymentStatusId = shipment.PaymentStatusId,
                            TotalWeight = shipment.TotalWeight,
                            OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal),
                            OrderId = order.Id,
                            CustomOrderNumber = order.CustomOrderNumber,
                            TotalShipmentAmount = _priceFormatter.FormatPrice(decimal.Parse(shipment.TotalAmount.ToString())),
                            TotalShipmentAmountPaid = _priceFormatter.FormatPrice(decimal.Parse(shipment.AmountPaid.ToString())),
                            InvoiceDueDate = shipment.InvoiceDueDateUtc,
                            InvoicePayedDate = shipment.InvoicePayedDateUtc
                        };


                        var customValues = _paymentService.DeserializeCustomValues(order);

                        if (customValues != null)
                        {
                            foreach (var item in customValues)
                            {
                                if (item.Key == "PO Number")
                                    paymentShipmentListModel.PurchaseOrderNumber = item.Value.ToString();
                            }
                        }

                        paymentShipmentListModel.TotalShipmentOpenAmount = _priceFormatter.FormatPrice(decimal.Parse(shipment.TotalAmount.ToString()) - decimal.Parse(shipment.AmountPaid.ToString()));
                        totalOpenAmount += shipment.TotalAmount - shipment.AmountPaid;
                        totalInvoicedAmount += shipment.AmountPaid;

                        var currentShipmentOpenAmount = shipment.TotalAmount - shipment.AmountPaid;

                        var invoiceDueDate = paymentShipmentListModel.InvoiceDueDate;
                        
                        if(shipment.PaymentStatusId != (int)PaymentStatus.Paid && invoiceDueDate != null)
                        {
                            if (DateTime.UtcNow > DateTime.Parse(invoiceDueDate.ToString()) && customer.PaymentTermsId == (int)PaymentTerms.Net30)
                            {
                                pastdueDate += decimal.Parse(currentShipmentOpenAmount.ToString());
                                pastdueDate30 += decimal.Parse(currentShipmentOpenAmount.ToString());
                            }
                            else if (DateTime.UtcNow > DateTime.Parse(invoiceDueDate.ToString()) && customer.PaymentTermsId == (int)PaymentTerms.Net60)
                            {
                                pastdueDate += decimal.Parse(currentShipmentOpenAmount.ToString());
                                pastdueDate60 += decimal.Parse(currentShipmentOpenAmount.ToString());
                            }
                            else if (DateTime.UtcNow > DateTime.Parse(invoiceDueDate.ToString()) && customer.PaymentTermsId == (int)PaymentTerms.Net90)
                            {
                                pastdueDate += decimal.Parse(currentShipmentOpenAmount.ToString());
                                pastdueDate90 += decimal.Parse(currentShipmentOpenAmount.ToString());
                            }
                        }
                        model.PaymentShipmentListModel.Add(paymentShipmentListModel);
                        model.LoggedInUser = _commonModelFactory.PrepareAdminHeaderLinksModel().ImpersonatedCustomerName;
                    }
                }
                if (billingAddress.ParentAddressId == customer.BillingAddressId)
                    customerTotalAmount += order.OrderTotal;
            }

            model.CustomerTotalInvoicedAmount = _priceFormatter.FormatPrice(decimal.Parse(totalInvoicedAmount.ToString()));
            model.CustomerTotalOpenAmount = _priceFormatter.FormatPrice(decimal.Parse(totalOpenAmount.ToString()));
            model.CustomerTotalAmount = _priceFormatter.FormatPrice(decimal.Parse(customerTotalAmount.ToString()));
            model.PastDue = _priceFormatter.FormatPrice(decimal.Parse(pastdueDate.ToString()));
            model.DaysPastDue30 = _priceFormatter.FormatPrice(decimal.Parse(pastdueDate30.ToString()));
            model.DaysPastDue60 = _priceFormatter.FormatPrice(decimal.Parse(pastdueDate60.ToString()));
            model.DaysPastDue90 = _priceFormatter.FormatPrice(decimal.Parse(pastdueDate90.ToString()));
            return model;
        }

        #endregion
    }
}