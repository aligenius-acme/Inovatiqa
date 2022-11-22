using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Payments
{
    public partial class PostProcessPaymentRequest
    {
        public Order Order { get; set; }
    }
}
