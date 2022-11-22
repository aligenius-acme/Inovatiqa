using Inovatiqa.Services.Shipping.Interfaces;
using Inovatiqa.Services.Shipping.Tracking;
using Inovatiqa.Services.Shipping.Tracking.Interfaces;
using System.Collections.Generic;
namespace Inovatiqa.Services.Shipping
{
    public class FedexShipmentTrackerService : IShipmentTrackerService
    {
        #region Fields

        private readonly IFedexService _fedexService;

        #endregion

        #region Ctor

        public FedexShipmentTrackerService(IFedexService fedexService)
        {
            _fedexService = fedexService;
        }

        #endregion

        #region Methods

        public virtual bool IsMatch(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return false;

            return false;
        }

        public virtual string GetUrl(string trackingNumber)
        {
            return $"https://www.fedex.com/apps/fedextrack/?action=track&tracknumbers={trackingNumber}";
        }

        public virtual IList<ShipmentStatusEvent> GetShipmentEvents(string trackingNumber)
        {
            if (string.IsNullOrEmpty(trackingNumber))
                return new List<ShipmentStatusEvent>();

            return _fedexService.GetShipmentEvents(trackingNumber);
        }

        #endregion
    }
}