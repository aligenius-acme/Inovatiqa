using Inovatiqa.Core;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Shipping
{
    public partial class GetShippingOptionResponse
    {
        public GetShippingOptionResponse()
        {
            Errors = new List<string>();
            ShippingOptions = new List<ShippingOption>();
        }

        public IList<ShippingOption> ShippingOptions { get; set; }

        public bool ShippingFromMultipleLocations { get; set; }

        public IList<string> Errors { get; set; }

        public bool Success => !Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
        }
    }
}