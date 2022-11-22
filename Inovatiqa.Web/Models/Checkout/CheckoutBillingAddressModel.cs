using Inovatiqa.Web.Models.Common;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Checkout
{
    public partial class CheckoutBillingAddressModel
    {
        public CheckoutBillingAddressModel()
        {
            ExistingAddresses = new List<AddressModel>();
            InvalidExistingAddresses = new List<AddressModel>();
            BillingNewAddress = new AddressModel();
        }

        public IList<AddressModel> ExistingAddresses { get; set; }
        public IList<AddressModel> InvalidExistingAddresses { get; set; }

        public AddressModel BillingNewAddress { get; set; }

        public bool ShipToSameAddress { get; set; }
        public bool ShipToSameAddressAllowed { get; set; }

        public bool NewAddressPreselected { get; set; }
    }
}