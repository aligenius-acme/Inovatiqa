using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Inovatiqa.Core;
using Inovatiqa.Core.Http.Extensions;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Payments.Interfaces;
using Microsoft.AspNetCore.Http;
using Square.Models;
using Square.Exceptions;

namespace Inovatiqa.Services.Payments
{
    public partial class PaymentService : IPaymentService
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly Lazy<IOrderTotalCalculationService> _orderTotalCalculationService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public PaymentService(ICustomerService customerService,
            Lazy<IOrderTotalCalculationService> orderTotalCalculationService,
            IPaymentMethodService paymentMethodService,
            IPriceCalculationService priceCalculationService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor)
        {
            _customerService = customerService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentMethodService = paymentMethodService;
            _priceCalculationService = priceCalculationService;
            _httpContextAccessor = httpContextAccessor;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        public virtual CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            return _paymentMethodService.Capture(capturePaymentRequest);
        }

        public virtual RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            return _paymentMethodService.Refund(refundPaymentRequest);
        }

        public virtual ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest.OrderTotal == decimal.Zero)
            {
                var result = new ProcessPaymentResult
                {
                    NewPaymentStatus = PaymentStatus.Paid
                };
                return result;
            }
            else if (!string.IsNullOrWhiteSpace(processPaymentRequest.CreditCardNumber))
            {
                processPaymentRequest.CreditCardNumber = processPaymentRequest.CreditCardNumber.Replace(" ", string.Empty);
                processPaymentRequest.CreditCardNumber = processPaymentRequest.CreditCardNumber.Replace("-", string.Empty);
            }

            //var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            if(processPaymentRequest.PaymentMethodSystemName != InovatiqaDefaults.PurchaseOrderPaymentName)
                return _paymentMethodService.ProcessPayment(processPaymentRequest);
            else
                return _paymentMethodService.ProcessPurchaseOrderPayment(processPaymentRequest);
        }

        public virtual decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart, string paymentMethodSystemName)
        {
            if (string.IsNullOrEmpty(paymentMethodSystemName))
                return decimal.Zero;

            var customer = _customerService.GetCustomerById(cart.FirstOrDefault()?.CustomerId ?? 0);


            var result = _paymentMethodService.GetAdditionalHandlingFee(cart);
            if (result < decimal.Zero)
                result = decimal.Zero;

            if (!InovatiqaDefaults.RoundPricesDuringCalculation)
                return result;

            result = _priceCalculationService.RoundPrice(result);

            return result;
        }

        public virtual decimal CalculateAdditionalFee(IList<ShoppingCartItem> cart, decimal fee, bool usePercentage)
        {
            if (fee <= 0)
                return fee;

            decimal result;
            if (usePercentage)
            {
                var orderTotalWithoutPaymentFee = _orderTotalCalculationService.Value.GetShoppingCartTotal(cart, usePaymentMethodAdditionalFee: false);
                result = (decimal)((float)orderTotalWithoutPaymentFee * (float)fee / 100f);
            }
            else
            {
                result = fee;
            }

            return result;
        }

        public virtual void GenerateOrderGuid(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                return;

            var previousPaymentRequest = _httpContextAccessor.HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
            if (InovatiqaDefaults.RegenerateOrderGuidInterval > 0 &&
                previousPaymentRequest != null &&
                previousPaymentRequest.OrderGuidGeneratedOnUtc.HasValue)
            {
                var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < InovatiqaDefaults.RegenerateOrderGuidInterval)
                {
                    processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                    processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                }
            }

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.OrderGuidGeneratedOnUtc = DateTime.UtcNow;
            }
        }

        public virtual int GetRecurringPaymentType(string paymentMethodSystemName)
        {
            return _paymentMethodService.RecurringPaymentType;
        }

        public virtual ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest.OrderTotal == decimal.Zero)
            {
                var result = new ProcessPaymentResult
                {
                    NewPaymentStatus = PaymentStatus.Paid
                };
                return result;
            }

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            return _paymentMethodService.ProcessRecurringPayment(processPaymentRequest);
        }

        public virtual string GetMaskedCreditCardNumber(string creditCardNumber)
        {
            if (string.IsNullOrEmpty(creditCardNumber))
                return string.Empty;

            if (creditCardNumber.Length <= 4)
                return creditCardNumber;

            var last4 = creditCardNumber.Substring(creditCardNumber.Length - 4, 4);
            var maskedChars = string.Empty;
            for (var i = 0; i < creditCardNumber.Length - 4; i++)
            {
                maskedChars += "*";
            }

            return maskedChars + last4;
        }

        public virtual string SerializeCustomValues(ProcessPaymentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!request.CustomValues.Any())
                return null;

            var ds = new DictionarySerializer(request.CustomValues);
            var xs = new XmlSerializer(typeof(DictionarySerializer));

            using var textWriter = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(textWriter))
            {
                xs.Serialize(xmlWriter, ds);
            }

            var result = textWriter.ToString();
            return result;
        }

        public virtual Dictionary<string, object> DeserializeCustomValues(Database.Models.Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (string.IsNullOrWhiteSpace(order.CustomValuesXml))
                return new Dictionary<string, object>();

            var serializer = new XmlSerializer(typeof(DictionarySerializer));

            using var textReader = new StringReader(order.CustomValuesXml);
            using var xmlReader = XmlReader.Create(textReader);
            if (serializer.Deserialize(xmlReader) is DictionarySerializer ds)
                return ds.Dictionary;
            return new Dictionary<string, object>();
        }

        public virtual void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest.Order.PaymentStatusId == (int)PaymentStatus.Paid)
                return;

            var customer = _customerService.GetCustomerById(postProcessPaymentRequest.Order.CustomerId);

            _paymentMethodService.PostProcessPayment(postProcessPaymentRequest);
        }

        public virtual bool CanRePostProcessPayment(Database.Models.Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!InovatiqaDefaults.AllowRePostingPayments)
                return false;

            var customer = _customerService.GetCustomerById(order.CustomerId);


            if (_paymentMethodService.PaymentMethodType != (int)PaymentMethodType.Redirection)
                return false;   //this option is available only for redirection payment methods

            if (order.Deleted)
                return false;  //do not allow for deleted orders

            if (order.OrderStatusId == (int)OrderStatus.Cancelled)
                return false;  //do not allow for cancelled orders

            if (order.PaymentStatusId != (int)PaymentStatus.Pending)
                return false;  //payment status should be Pending

            return _paymentMethodService.CanRePostProcessPayment(order);
        }

        public virtual bool SupportCapture(string paymentMethodSystemName)
        {
            if (paymentMethodSystemName == InovatiqaDefaults.PurchaseOrderPaymentName)
                return false;
            else
                return true;
        }

        public virtual bool SupportRefund(string paymentMethodSystemName)
        {
            if (paymentMethodSystemName == InovatiqaDefaults.PurchaseOrderPaymentName)
                return false;
            else
                return true;
        }

        public virtual bool SupportPartiallyRefund(string paymentMethodSystemName)
        {
            if (paymentMethodSystemName == InovatiqaDefaults.PurchaseOrderPaymentName)
                return false;
            else
                return true;
        }

        public virtual bool SupportVoid(string paymentMethodSystemName)
        {
            if (paymentMethodSystemName == InovatiqaDefaults.PurchaseOrderPaymentName)
                return false;
            else
                return true;
        }

        public virtual VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            return _paymentMethodService.Void(voidPaymentRequest);
        }

        public virtual ProcessPaymentResult ProcessShipmentPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return _paymentMethodService.ProcessShipmentPayment(processPaymentRequest);
        }

        public virtual string CreateCustomerCard(string cardNonce, Database.Models.Customer customer)
        {
           return _paymentMethodService.CreateCustomerCard(cardNonce, customer);
        }

        public virtual async Task<CreatePaymentResponse> ProcessACHBankPayment(string Token, string Amount)
        {
            var PaymentModel = new PaymentInfoModel();
            var PaymentsApi = PaymentModel.client.PaymentsApi;
            string uuid = NewIdempotencyKey();

            var retrieveLocationResponse = await PaymentModel.client.LocationsApi.RetrieveLocationAsync(InovatiqaDefaults.LocationId);
            var currency = retrieveLocationResponse.Location.Currency;
            var DeductionAmount = Convert.ToDouble(Amount) * 100; // amount must be in scents
            var PaymentAmount = new Money.Builder()
            .Amount((long)DeductionAmount)
            .Currency(currency)
            .Build();

            var createPaymentRequest = new CreatePaymentRequest.Builder(
            sourceId: Token,
            idempotencyKey: uuid,
            amountMoney: PaymentAmount)
            .Build();

            try
            {
                var response = await PaymentsApi.CreatePaymentAsync(createPaymentRequest);
                //return new JsonResult(new { payment = response.Payment });
                return response;
            }
            catch (ApiException e)
            {
                throw e;
                //return new JsonResult(new { errors = e.Errors });
            }
        }
        private string NewIdempotencyKey()
        {
            return Guid.NewGuid().ToString();
        }
        #endregion
    }
}