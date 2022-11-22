using Inovatiqa.Database.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Inovatiqa.Services.Payments.Interfaces
{
    public partial interface IPaymentMethodService
    {
        #region Methods

        CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest);

        decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart);

        IList<string> ValidatePaymentForm(IFormCollection form);

        ProcessPaymentRequest GetPaymentInfo(IFormCollection form);

        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest);

        ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest);

        void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest);

        bool CanRePostProcessPayment(Order order);

        ProcessPaymentResult ProcessPurchaseOrderPayment(ProcessPaymentRequest processPaymentRequest);

        ProcessPaymentRequest GetPaymentInfoPO(IFormCollection form);

        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest);

        VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest);

        ProcessPaymentResult ProcessShipmentPayment(ProcessPaymentRequest processPaymentRequest);

        string CreateCustomerCard(string cardNonce, Customer customer);

        #endregion

        #region Properties

        bool SkipPaymentInfo { get; }

        int RecurringPaymentType { get; }

        int PaymentMethodType { get; }

        #endregion
    }
}
