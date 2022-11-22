using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Payments
{
    public partial class RefundPaymentRequest
    {
        public Order Order { get; set; }

        public decimal AmountToRefund { get; set; }

        public bool IsPartialRefund { get; set; }
    }
}
