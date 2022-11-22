using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Web.Models.Common;
using System.Collections.Generic;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class CustomerAddressListModel : BaseInovatiqaModel
    {
        public CustomerAddressListModel()
        {
            Addresses = new List<AddressModel>();
        }
        public bool canEditMainAddresses { get; set; }
        public int HasSubAccount { get; set; }
        public IList<AddressModel> Addresses { get; set; }
    }
}