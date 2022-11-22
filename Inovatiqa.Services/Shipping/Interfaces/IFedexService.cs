using Inovatiqa.Services.Shipping.Tracking;
using System.Collections.Generic;

namespace Inovatiqa.Services.Shipping.Interfaces
{
    public interface IFedexService
    {
        IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber);

        GetShippingOptionResponse GetRates(GetShippingOptionRequest shippingOptionRequest);
    }
}
