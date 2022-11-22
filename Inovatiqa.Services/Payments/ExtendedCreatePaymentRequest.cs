using System.Runtime.Serialization;
using Square.Models;

namespace Inovatiqa.Payments
{
    public class ExtendedCreatePaymentRequest : CreatePaymentRequest
    {
        #region Ctor

        public ExtendedCreatePaymentRequest(
            string sourceId, 
            string idempotencyKey, 
            Money amountMoney, 
            Money tipMoney = null, 
            Money appFeeMoney = null,
            string integrationId = null,
            string delayDuration = null, 
            bool? autocomplete = null, 
            string orderId = null, 
            string customerId = null, 
            string locationId = null, 
            string referenceId = null, 
            string verificationToken = null, 
            bool? acceptPartialAuthorization = null, 
            string buyerEmailAddress = null, 
            Address billingAddress = null, 
            Address shippingAddress = null, 
            string note = null, 
            string statementDescriptionIdentifier = null) 
            : base(
                sourceId, 
                idempotencyKey, 
                amountMoney, 
                tipMoney, 
                appFeeMoney, 
                delayDuration, 
                autocomplete, 
                orderId, 
                customerId, 
                locationId, 
                referenceId, 
                verificationToken, 
                acceptPartialAuthorization, 
                buyerEmailAddress, 
                billingAddress, 
                shippingAddress, 
                note, 
                statementDescriptionIdentifier)
        {
            IntegrationId = integrationId;
        }

        #endregion

        #region Properties

        [DataMember(Name = "integration_id", EmitDefaultValue = false)]
        public string IntegrationId { get; set; }

        #endregion
    }
}