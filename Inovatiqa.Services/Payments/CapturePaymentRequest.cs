using Inovatiqa.Database.Models;

namespace Inovatiqa.Services.Payments
{
    public partial class CapturePaymentRequest
    {
        public Order Order { get; set; }

        public string ShipmentAuthorizationId { get; set; }
    }
}
