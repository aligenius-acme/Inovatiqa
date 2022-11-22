using Inovatiqa.Services.Shipping.Tracking.Interfaces;

namespace Inovatiqa.Services.Shipping.Interfaces
{
    public partial interface IShippingRateComputationMethodService
    {
        int ShippingRateComputationMethodType { get; }

        GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest);

        decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest);

        IShipmentTrackerService ShipmentTracker { get; }
    }
}