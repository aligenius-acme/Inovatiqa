using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Payments
{
    [Serializable]
    public partial class ProcessPaymentRequest
    {
        public ProcessPaymentRequest()
        {
            CustomValues = new Dictionary<string, object>();
        }

        public int StoreId { get; set; }

        public int CustomerId { get; set; }

        public Guid OrderGuid { get; set; }
        
        public DateTime? OrderGuidGeneratedOnUtc { get; set; }

        public decimal OrderTotal { get; set; }

        public string PaymentMethodSystemName { get; set; }

        #region Payment method specific properties 

        public string CreditCardType { get; set; }

        public string CreditCardName { get; set; }

        public string CreditCardNumber { get; set; }

        public int CreditCardExpireYear { get; set; }

        public int CreditCardExpireMonth { get; set; }

        public string CreditCardCvv2 { get; set; }

        public string CardNonce { get; set; }

        #endregion

        #region Recurring payments

        public Order InitialOrder { get; set; }

        public int RecurringCycleLength { get; set; }

        public int RecurringCyclePeriodId { get; set; }

        public int RecurringTotalCycles { get; set; }

        #endregion

        public Dictionary<string, object> CustomValues { get; set; }
    }
}