using Inovatiqa.Core;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Payments
{
    public partial class ProcessPaymentResult
    {
        public ProcessPaymentResult()
        {
            Errors = new List<string>();
        }

        public bool Success => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public IList<string> Errors { get; set; }

        public string AvsResult { get; set; }

        public string Cvv2Result { get; set; }

        public string AuthorizationTransactionId { get; set; }

        public string AuthorizationTransactionCode { get; set; }

        public string AuthorizationTransactionResult { get; set; }

        public string CaptureTransactionId { get; set; }

        public string CaptureTransactionResult { get; set; }

        public string SubscriptionTransactionId { get; set; }

        public bool AllowStoringCreditCardNumber { get; set; }

        public bool RecurringPaymentFailed { get; set; }

        public PaymentStatus NewPaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}