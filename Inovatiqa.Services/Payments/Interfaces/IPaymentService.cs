using Inovatiqa.Database.Models;
using Square.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inovatiqa.Services.Payments.Interfaces
{
    public partial interface IPaymentService
    {
        CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest);

        decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart, string paymentMethodSystemName);

        decimal CalculateAdditionalFee(IList<ShoppingCartItem> cart, decimal fee, bool usePercentage);

        void GenerateOrderGuid(ProcessPaymentRequest processPaymentRequest);

        int GetRecurringPaymentType(string paymentMethodSystemName);

        ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest);

        ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest);

        string GetMaskedCreditCardNumber(string creditCardNumber);

        string SerializeCustomValues(ProcessPaymentRequest request);

        Dictionary<string, object> DeserializeCustomValues(Database.Models.Order order);

        void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest);

        bool CanRePostProcessPayment(Database.Models.Order order);

        bool SupportCapture(string paymentMethodSystemName);

        bool SupportRefund(string paymentMethodSystemName);

        bool SupportPartiallyRefund(string paymentMethodSystemName);

        bool SupportVoid(string paymentMethodSystemName);

        RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest);

        VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest);

        ProcessPaymentResult ProcessShipmentPayment(ProcessPaymentRequest processPaymentRequest);

        string CreateCustomerCard(string cardNonce, Database.Models.Customer customer);
        Task<CreatePaymentResponse> ProcessACHBankPayment(string Token, string Amount);
    }
}