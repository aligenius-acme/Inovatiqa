using Inovatiqa.Core;
using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Shipping.Tracking.Interfaces;
using System;
using System.Linq;

namespace Inovatiqa.Services.Shipping
{
    public class FedexComputationMethodService : IShippingRateComputationMethodService
    {
        #region Fields

        private readonly IFedexService _fedexService;

        #endregion

        #region Ctor

        public FedexComputationMethodService(IFedexService fedexService)
        {
            _fedexService = fedexService;
        }

        #endregion

        #region Methods

        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest is null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            if (!getShippingOptionRequest.Items?.Any() ?? true)
                return new GetShippingOptionResponse { Errors = new[] { "No shipment items" } };

            if (getShippingOptionRequest.ShippingAddress?.CountryId is null)
                return new GetShippingOptionResponse { Errors = new[] { "Shipping address is not set" } };

            return _fedexService.GetRates(getShippingOptionRequest);
        }

        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        #endregion

        #region Properties

        public int ShippingRateComputationMethodType => InovatiqaDefaults.Realtime;

        public IShipmentTrackerService ShipmentTracker => new FedexShipmentTrackerService(_fedexService);

        #endregion
    }
}