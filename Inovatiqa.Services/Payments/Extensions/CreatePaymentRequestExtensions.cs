using Square.Models;

namespace Inovatiqa.Payments.Extensions
{
    public static class CreatePaymentRequestExtensions
    {
        #region Methods

        public static ExtendedCreatePaymentRequest ToExtendedRequest(this CreatePaymentRequest paymentRequest, string integrationId)
        {
            return new ExtendedCreatePaymentRequest(sourceId: paymentRequest.SourceId,
                idempotencyKey: paymentRequest.IdempotencyKey,
                amountMoney: paymentRequest.AmountMoney,
                tipMoney: paymentRequest.TipMoney,
                appFeeMoney: paymentRequest.AppFeeMoney,
                integrationId: integrationId,
                delayDuration: paymentRequest.DelayDuration,
                autocomplete: paymentRequest.Autocomplete,
                orderId: paymentRequest.OrderId,
                customerId: paymentRequest.CustomerId,
                locationId: paymentRequest.LocationId,
                referenceId: paymentRequest.ReferenceId,
                verificationToken: paymentRequest.VerificationToken,
                acceptPartialAuthorization: paymentRequest.AcceptPartialAuthorization,
                buyerEmailAddress: paymentRequest.BuyerEmailAddress,
                billingAddress: paymentRequest.BillingAddress,
                shippingAddress: paymentRequest.ShippingAddress,
                note: paymentRequest.Note,
                statementDescriptionIdentifier: paymentRequest.StatementDescriptionIdentifier);
        }

        #endregion
    }
}