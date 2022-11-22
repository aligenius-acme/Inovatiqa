using System;

namespace Inovatiqa.Services.Shipping.Tracking
{
    public partial class ShipmentStatusEvent
    {
        public string EventName { get; set; }

        public string Location { get; set; }

        public string CountryCode { get; set; }

        public DateTime? Date { get; set; }
    }
}
