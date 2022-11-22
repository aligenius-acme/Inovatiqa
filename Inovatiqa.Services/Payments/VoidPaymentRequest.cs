using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Payments
{
    public partial class VoidPaymentRequest
    {
        public Order Order { get; set; }
    }
}
