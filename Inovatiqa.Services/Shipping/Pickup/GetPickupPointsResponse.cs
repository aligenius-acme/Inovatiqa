using Inovatiqa.Core;
using System.Collections.Generic;

namespace Inovatiqa.Services.Shipping.Pickup
{
    public partial class GetPickupPointsResponse
    {
        public GetPickupPointsResponse()
        {
            Errors = new List<string>();
            PickupPoints = new List<PickupPoint>();
        }

        public IList<PickupPoint> PickupPoints { get; set; }

        public IList<string> Errors { get; set; }

        public bool Success => Errors.Count == 0;

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}