using Inovatiqa.Core;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Payments
{
    public partial class VoidPaymentResult
    {
        public VoidPaymentResult()
        {
            Errors = new List<string>();
        }

        public bool Success => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public IList<string> Errors { get; set; }

        public PaymentStatus NewPaymentStatus { get; set; } = PaymentStatus.Pending;
    }
}