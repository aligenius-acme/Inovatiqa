using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Payments
{
    public partial class CancelRecurringPaymentResult
    {
        public CancelRecurringPaymentResult()
        {
            Errors = new List<string>();
        }

        public bool Success => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public IList<string> Errors { get; set; }
    }
}