using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Payments
{
    public partial class CancelRecurringPaymentRequest
    {
        public Order Order { get; set; }
    }
}
