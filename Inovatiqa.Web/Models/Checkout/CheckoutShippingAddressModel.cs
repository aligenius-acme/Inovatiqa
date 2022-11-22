using Inovatiqa.Web.Models.Common;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Checkout
{
    public partial class CheckoutShippingAddressModel
    {
        public CheckoutShippingAddressModel()
        {
            ExistingAddresses = new List<AddressModel>();
            InvalidExistingAddresses = new List<AddressModel>();
            ShippingNewAddress = new AddressModel();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }
        public IList<AddressModel> InvalidExistingAddresses { get; set; }
        public AddressModel ShippingNewAddress { get; set; }
        public bool NewAddressPreselected { get; set; }

        public bool DisplayPickupInStore { get; set; }
        public CheckoutPickupPointsModel PickupPointsModel { get; set; }
    }
}