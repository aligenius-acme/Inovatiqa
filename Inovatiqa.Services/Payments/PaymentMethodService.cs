using Inovatiqa.Core;
using Inovatiqa.Database.Models;
using Inovatiqa.Payments;
using Inovatiqa.Payments.Extensions;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Directory.Interfaces;
using Inovatiqa.Services.Logging.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SquareModel = Square.Models;

namespace Inovatiqa.Services.Payments
{
    public partial class PaymentMethodService : IPaymentMethodService
    {
        #region Fields

        private readonly Lazy<IPaymentService> _paymentService;
        private readonly ICustomerService _customerService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILoggerService _loggerService;
        private readonly ISquarePaymentManagerService _squarePaymentManagerService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public PaymentMethodService(Lazy<IPaymentService> paymentService,
            ICustomerService customerService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ILoggerService loggerService,
            ISquarePaymentManagerService squarePaymentManagerService,
            IGenericAttributeService genericAttributeService)
        {
            _paymentService = paymentService;
            _customerService = customerService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _loggerService = loggerService;
            _squarePaymentManagerService = squarePaymentManagerService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion


        #region Utilities

        private PaymentStatus GetPaymentStatus(string status)
        {
            return status switch
            {
                InovatiqaDefaults.PAYMENT_APPROVED_STATUS => PaymentStatus.Authorized,
                InovatiqaDefaults.PAYMENT_COMPLETED_STATUS => PaymentStatus.Paid,
                InovatiqaDefaults.PAYMENT_FAILED_STATUS => PaymentStatus.Pending,
                InovatiqaDefaults.PAYMENT_CANCELED_STATUS => PaymentStatus.Voided,
                _ => PaymentStatus.Pending,
            };
        }

        private ExtendedCreatePaymentRequest CreatePaymentRequest(ProcessPaymentRequest paymentRequest, bool isRecurringPayment)
        {
            var customer = _customerService.GetCustomerById(paymentRequest.CustomerId);
            if (customer == null)
                throw new InovatiqaException("Customer cannot be loaded");

            var currency = InovatiqaDefaults.CurrencyCode;
            if (currency == null)
                throw new InovatiqaException("Primary store currency cannot be loaded");

            var storeId = InovatiqaDefaults.StoreId;

            SquareModel.Address createAddress(Address address)
            {
                if (address == null)
                    return null;

                var country = _countryService.GetCountryByAddress(address);

                return new SquareModel.Address
                (
                    addressLine1: address.Address1,
                    addressLine2: address.Address2,
                    administrativeDistrictLevel1: _stateProvinceService.GetStateProvinceByAddress(address)?.Abbreviation,
                    administrativeDistrictLevel2: address.County,
                    country: string.Equals(country?.TwoLetterIsoCode, new RegionInfo(country?.TwoLetterIsoCode).TwoLetterISORegionName, StringComparison.InvariantCultureIgnoreCase)
                        ? country?.TwoLetterIsoCode : null,
                    firstName: address.FirstName,
                    lastName: address.LastName,
                    locality: address.City,
                    postalCode: address.ZipPostalCode
                );
            }

            var customerBillingAddress = _customerService.GetCustomerBillingAddress(customer);
            var customerShippingAddress = _customerService.GetCustomerShippingAddress(customer);

            var billingAddress = createAddress(customerBillingAddress);
            var shippingAddress = billingAddress == null ? createAddress(customerShippingAddress) : null;
            var email = customerBillingAddress != null ? customerBillingAddress.Email : customerShippingAddress?.Email;

            if ((billingAddress == null && shippingAddress == null) || string.IsNullOrEmpty(email))
                _loggerService.Warning("Square payment warning: Address or email is not provided, so the transaction is ineligible for chargeback protection", customer: customer);

            var orderTotal = (int)(paymentRequest.OrderTotal * 100);
            var amountMoney = new SquareModel.Money(orderTotal, currency);

            var tokenKey = "Verification token";
            if ((!paymentRequest.CustomValues.TryGetValue(tokenKey, out var token) || string.IsNullOrEmpty(token?.ToString())) && InovatiqaDefaults.Use3ds)
                throw new InovatiqaException("Failed to get the verification token");

            paymentRequest.CustomValues.Remove(tokenKey);

            var location = _squarePaymentManagerService.GetSelectedActiveLocation(storeId);
            if (location == null)
                throw new InovatiqaException("Location is a required parameter for payment requests");

            var paymentRequestBuilder = new SquareModel.CreatePaymentRequest.Builder
                (
                    sourceId: null,
                    idempotencyKey: Guid.NewGuid().ToString(),
                    amountMoney: amountMoney
                )
                .Autocomplete(false)
                .BillingAddress(billingAddress)
                .ShippingAddress(shippingAddress)
                .BuyerEmailAddress(email)
                .Note(string.Format(InovatiqaDefaults.PaymentNote, paymentRequest.OrderGuid))
                .ReferenceId(paymentRequest.OrderGuid.ToString())
                //.VerificationToken(token?.ToString())
                .LocationId(location.Id);

            var integrationId = !InovatiqaDefaults.UseSandbox && !string.IsNullOrEmpty(InovatiqaDefaults.IntegrationId)
                ? InovatiqaDefaults.IntegrationId
                : null;

            var storedCardKey = "Pay using stored card token";
            if (paymentRequest.CustomValues.TryGetValue(storedCardKey, out var storedCardId) && !storedCardId.ToString().Equals(Guid.Empty.ToString()))
            {
                var customerId = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CustomerIdAttribute, customer.Id);
                var squareCustomer = _squarePaymentManagerService.GetCustomer(customerId, storeId);
                if (squareCustomer == null)
                    throw new InovatiqaException("Failed to retrieve customer");

                return paymentRequestBuilder
                    .CustomerId(squareCustomer.Id)
                    .SourceId(storedCardId.ToString())
                    .Build()
                    .ToExtendedRequest(integrationId);
            }

            var cardNonceKey = "Pay using card nonce";
            if (!paymentRequest.CustomValues.TryGetValue(cardNonceKey, out var cardNonce) || string.IsNullOrEmpty(cardNonce?.ToString()))
                throw new InovatiqaException("Failed to get the card nonce");

            paymentRequest.CustomValues.Remove(cardNonceKey);

            var saveCardKey = "Save card details";
            if (paymentRequest.CustomValues.TryGetValue(saveCardKey, out var saveCardValue) && saveCardValue is bool saveCard && saveCard && !_customerService.IsGuest(customer))
            {
                paymentRequest.CustomValues.Remove(saveCardKey);

                try
                {
                    var customerId = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CustomerIdAttribute, customer.Id);
                    var squareCustomer = _squarePaymentManagerService.GetCustomer(customerId, storeId);

                    if (squareCustomer == null)
                    {
                        var customerRequestBuilder = new SquareModel.CreateCustomerRequest.Builder()
                            .EmailAddress(customer.Email)
                            .Nickname(customer.Username)
                            .GivenName(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id))
                            .FamilyName(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id))
                            .PhoneNumber(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PhoneAttribute, customer.Id))
                            .CompanyName(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id))
                            .ReferenceId(customer.CustomerGuid.ToString());

                        squareCustomer = _squarePaymentManagerService.CreateCustomer(customerRequestBuilder.Build(), storeId);
                        if (squareCustomer == null)
                            throw new InovatiqaException("Failed to create customer. Error details in the log");

                        _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CustomerIdAttribute, squareCustomer.Id);
                    }

                    //var cardRequestBuilder = new SquareModel.CreateCustomerCardRequest.Builder(cardNonce.ToString())
                    //    .VerificationToken(token?.ToString());

                    var cardRequestBuilder = new SquareModel.CreateCustomerCardRequest.Builder(cardNonce.ToString());

                    var cardBillingAddress = billingAddress ?? shippingAddress;

                    var postalCodeKey = "Postal code";
                    if (paymentRequest.CustomValues.TryGetValue(postalCodeKey, out var postalCode) && !string.IsNullOrEmpty(postalCode.ToString()))
                    {
                        paymentRequest.CustomValues.Remove(postalCodeKey);

                        cardBillingAddress ??= new SquareModel.Address();
                        cardBillingAddress = cardBillingAddress
                            .ToBuilder()
                            .PostalCode(postalCode.ToString())
                            .Build();
                    }

                    cardRequestBuilder.BillingAddress(cardBillingAddress);

                    var card = _squarePaymentManagerService.CreateCustomerCard(squareCustomer.Id, cardRequestBuilder.Build(), storeId);
                    if (card == null)
                        throw new InovatiqaException("Failed to create card. Error details in the log");

                    if (isRecurringPayment)
                        paymentRequest.CustomValues.Add(storedCardKey, card.Id);

                    return paymentRequestBuilder
                        .CustomerId(squareCustomer.Id)
                        .SourceId(card.Id)
                        .Build()
                        .ToExtendedRequest(integrationId);
                }
                catch (Exception exception)
                {
                    _loggerService.Warning(exception.Message, exception, customer);
                    if (isRecurringPayment)
                        throw new InovatiqaException("For recurring payments you need to save the card details");
                }
            }
            else if (isRecurringPayment)
                throw new InovatiqaException("For recurring payments you need to save the card details");

            return paymentRequestBuilder
                .SourceId(cardNonce.ToString())
                .Build()
                .ToExtendedRequest(integrationId);
        }

        private ExtendedCreatePaymentRequest CreateShipmentPaymentRequest(ProcessPaymentRequest paymentRequest)
        {
            var customer = _customerService.GetCustomerById(paymentRequest.CustomerId);
            if (customer == null)
                throw new InovatiqaException("Customer cannot be loaded");

            var currency = InovatiqaDefaults.CurrencyCode;
            if (currency == null)
                throw new InovatiqaException("Primary store currency cannot be loaded");

            var orderTotal = (int)(paymentRequest.OrderTotal * 100);
            var amountMoney = new SquareModel.Money(orderTotal, currency);

            var paymentRequestBuilder = new SquareModel.CreatePaymentRequest.Builder
                (
                    sourceId: null,
                    idempotencyKey: Guid.NewGuid().ToString(),
                    amountMoney: amountMoney                
                )
                .Autocomplete(false)
                .Note(string.Format(InovatiqaDefaults.PaymentNote, paymentRequest.OrderGuid))
                .ReferenceId(paymentRequest.OrderGuid.ToString());

            var integrationId = !InovatiqaDefaults.UseSandbox && !string.IsNullOrEmpty(InovatiqaDefaults.IntegrationId)
                ? InovatiqaDefaults.IntegrationId
                : null;

            var customerId = _genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CustomerIdAttribute, customer.Id);
            var squareCustomer = _squarePaymentManagerService.GetCustomer(customerId, InovatiqaDefaults.StoreId);
            if (squareCustomer == null)
                throw new InovatiqaException("Failed to retrieve customer");


            return paymentRequestBuilder
                .CustomerId(squareCustomer.Id)
                .SourceId(paymentRequest.CardNonce)
                .Build()
                .ToExtendedRequest(integrationId);
        }

        private ProcessPaymentResult ProcessPayment(ProcessPaymentRequest paymentRequest, bool isRecurringPayment)
        {
            var squarePaymentRequest = CreatePaymentRequest(paymentRequest, isRecurringPayment);

            var storeId = InovatiqaDefaults.StoreId;
            var (payment, error) = _squarePaymentManagerService.CreatePayment(squarePaymentRequest, storeId);
            if (payment == null)
                throw new InovatiqaException(error);

            var paymentStatus = payment.Status;
            var paymentResult = $"Payment was processed. Status is {paymentStatus}";

            var result = new ProcessPaymentResult
            {
                NewPaymentStatus = GetPaymentStatus(paymentStatus)
            };

            result.CaptureTransactionId = payment.Id;
            result.CaptureTransactionResult = paymentResult;
            result.AuthorizationTransactionId = result.CaptureTransactionId;

            return result;
        }

        private ProcessPaymentResult ProcessShipmentPaymentRequest(ProcessPaymentRequest paymentRequest)
        {
            var squarePaymentRequest = CreateShipmentPaymentRequest(paymentRequest);

            var storeId = InovatiqaDefaults.StoreId;
            var (payment, error) = _squarePaymentManagerService.CreatePayment(squarePaymentRequest, storeId);
            if (payment == null)
                throw new InovatiqaException(error);

            var paymentStatus = payment.Status;
            var paymentResult = $"Payment was processed. Status is {paymentStatus}";

            var result = new ProcessPaymentResult
            {
                NewPaymentStatus = GetPaymentStatus(paymentStatus)
            };

            result.CaptureTransactionId = payment.Id;
            result.CaptureTransactionResult = paymentResult;
            result.AuthorizationTransactionId = result.CaptureTransactionId;

            return result;
        }

        #endregion

        #region Methods

        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            if (capturePaymentRequest == null)
                throw new ArgumentException(nameof(capturePaymentRequest));

            var storeId = InovatiqaDefaults.StoreId;
            string transactionId = null;
            if(capturePaymentRequest.Order != null)
                transactionId = capturePaymentRequest.Order.AuthorizationTransactionId;
            else if(capturePaymentRequest.ShipmentAuthorizationId != null)
                transactionId = capturePaymentRequest.ShipmentAuthorizationId;
            var (successfullyCompleted, error) = _squarePaymentManagerService.CompletePayment(transactionId, storeId);
            if (!successfullyCompleted)
                throw new InovatiqaException(error);

            return new CapturePaymentResult
            {
                NewPaymentStatus = PaymentStatus.Paid,
                CaptureTransactionId = transactionId
            };
        }

        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            return ProcessPayment(processPaymentRequest, false);
        }

        public virtual decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _paymentService.Value.CalculateAdditionalFee(cart,
                InovatiqaDefaults.AdditionalFee, InovatiqaDefaults.AdditionalFeePercentage);
        }

        public virtual IList<string> ValidatePaymentForm(IFormCollection form)
        {
            if (form.TryGetValue(nameof(PaymentInfoModel.Errors), out var errorsString) && !StringValues.IsNullOrEmpty(errorsString))
                return errorsString.ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return new List<string>();
        }

        public virtual ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentRequest = new ProcessPaymentRequest();

            if (form.TryGetValue(nameof(PaymentInfoModel.Token), out var token) && !StringValues.IsNullOrEmpty(token))
                paymentRequest.CustomValues.Add("Verification token", token.ToString());

            if (form.TryGetValue(nameof(PaymentInfoModel.CardNonce), out var cardNonce) && !StringValues.IsNullOrEmpty(cardNonce))
                paymentRequest.CustomValues.Add("Pay using card nonce", cardNonce.ToString());

            if (form.TryGetValue(nameof(PaymentInfoModel.StoredCardId), out var storedCardId) && !StringValues.IsNullOrEmpty(storedCardId) && !storedCardId.Equals(Guid.Empty.ToString()))
                paymentRequest.CustomValues.Add("Pay using stored card token", storedCardId.ToString());

            if (form.TryGetValue(nameof(PaymentInfoModel.SaveCard), out var saveCardValue) && !StringValues.IsNullOrEmpty(saveCardValue) && bool.TryParse(saveCardValue[0], out var saveCard) && saveCard)
                paymentRequest.CustomValues.Add("Save card details", saveCard);

            if (form.TryGetValue(nameof(PaymentInfoModel.PostalCode), out var postalCode) && !StringValues.IsNullOrEmpty(postalCode))
                paymentRequest.CustomValues.Add("Postal code", postalCode.ToString());

            return paymentRequest;
        }

        public virtual ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            return ProcessPayment(processPaymentRequest, true);
        }

        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //do nothing
        }

        public bool CanRePostProcessPayment(Order order)
        {
            return false;
        }

        public ProcessPaymentResult ProcessPurchaseOrderPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        public ProcessPaymentRequest GetPaymentInfoPO(IFormCollection form)
        {
            return new ProcessPaymentRequest
            {
                CustomValues = new Dictionary<string, object>
                {
                    ["PO Number"] = form["PurchaseOrderNumber"].ToString()
                }
            };
        }

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            if (refundPaymentRequest == null)
                throw new ArgumentException(nameof(refundPaymentRequest));

            var orderTotal = (int)(refundPaymentRequest.AmountToRefund * 100);
            var amountMoney = new SquareModel.Money(orderTotal, InovatiqaDefaults.CurrencyCode);

            var transactionId = refundPaymentRequest.Order.CaptureTransactionId;

            var paymentRefundRequest = new SquareModel.RefundPaymentRequest
                (
                    idempotencyKey: Guid.NewGuid().ToString(),
                    amountMoney: amountMoney,
                    paymentId: transactionId
                );

            var (paymentRefund, paymentRefundError) = _squarePaymentManagerService.RefundPayment(paymentRefundRequest, InovatiqaDefaults.StoreId);
            if (paymentRefund == null)
                throw new InovatiqaException(paymentRefundError);


            if (paymentRefund.Status == InovatiqaDefaults.REFUND_STATUS_PENDING)
            {
                System.Threading.Thread.Sleep(5000);
                (paymentRefund, paymentRefundError) = _squarePaymentManagerService.GetPaymentRefund(paymentRefund.Id, InovatiqaDefaults.StoreId);
                if (paymentRefund == null)
                    throw new InovatiqaException(paymentRefundError);
            }

            if (paymentRefund.Status != InovatiqaDefaults.REFUND_STATUS_COMPLETED)
            {
                return new RefundPaymentResult { Errors = new[] { $"Refund is {paymentRefund.Status}" }.ToList() };
            }

            return new RefundPaymentResult
            {
                NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded
            };
        }

        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            if (voidPaymentRequest == null)
                throw new ArgumentException(nameof(voidPaymentRequest));

            var transactionId = voidPaymentRequest.Order.AuthorizationTransactionId;
            var (successfullyCanceled, error) = _squarePaymentManagerService.CancelPayment(transactionId, InovatiqaDefaults.StoreId);
            if (!successfullyCanceled)
                throw new InovatiqaException(error);

            return new VoidPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Voided
            };
        }

        public ProcessPaymentResult ProcessShipmentPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            return ProcessShipmentPaymentRequest(processPaymentRequest);
        }

        public string CreateCustomerCard(string cardNonce, Customer customer)
        {
            if (customer == null)
            {
                return "error, Customer cannot be loaded";
                //throw new InovatiqaException("Customer cannot be loaded");
            }
                var customerId = _genericAttributeService
                .GetAttribute<string>(customer, InovatiqaDefaults.CustomerIdAttribute, customer.Id) ?? string.Empty;
            var storeId = InovatiqaDefaults.StoreId;
            try
            {
                var squareCustomer = _squarePaymentManagerService.GetCustomer(customerId, storeId);
                if (squareCustomer == null)
                {
                    var customerRequestBuilder = new SquareModel.CreateCustomerRequest.Builder()
                        .EmailAddress(customer.Email)
                        .Nickname(customer.Username)
                        .GivenName(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.FirstNameAttribute, customer.Id))
                        .FamilyName(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.LastNameAttribute, customer.Id))
                        .PhoneNumber(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.PhoneAttribute, customer.Id))
                        .CompanyName(_genericAttributeService.GetAttribute<string>(customer, InovatiqaDefaults.CompanyAttribute, customer.Id))
                        .ReferenceId(customer.CustomerGuid.ToString());

                    squareCustomer = _squarePaymentManagerService.CreateCustomer(customerRequestBuilder.Build(), storeId);
                    if (squareCustomer == null) 
                    {
                        return "error,Failed to create customer. Error details in the log";
                        //throw new InovatiqaException("Failed to create customer. Error details in the log");
                    }

                    _genericAttributeService.SaveAttribute(customer.GetType().Name, customer.Id, InovatiqaDefaults.CustomerIdAttribute, squareCustomer.Id);
                }

                var cardRequestBuilder = new SquareModel.CreateCustomerCardRequest.Builder(cardNonce);
                var card = _squarePaymentManagerService.CreateCustomerCard(squareCustomer.Id, cardRequestBuilder.Build(), storeId);
                if (card == null)
                {
                    return "error,Failed to create card. Error details in the log";
                   // throw new InovatiqaException("Failed to create card. Error details in the log");
                }
                return "success,Card Added Successfully";


            }
            catch (Exception exception)
            {
                _loggerService.Warning(exception.Message, exception, customer);
                return exception.Message;
            }
        }

        #endregion

        #region Properties

        public bool SkipPaymentInfo => false;

        public int RecurringPaymentType => InovatiqaDefaults.Manual;

        public int PaymentMethodType => InovatiqaDefaults.Standard;

        #endregion

    }
}
