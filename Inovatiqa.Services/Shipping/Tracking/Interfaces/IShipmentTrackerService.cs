using System.Collections.Generic;

namespace Inovatiqa.Services.Shipping.Tracking.Interfaces
{
    public partial interface IShipmentTrackerService
    {
        bool IsMatch(string trackingNumber);

        string GetUrl(string trackingNumber);

        IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber);
    }
}
