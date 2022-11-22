using System.Collections.Generic;
using Inovatiqa.Database.Models;

namespace Inovatiqa.Web.Models.Customer
{
    public class ChooseAddressModel
    {
        public ChooseAddressModel()
        {
            DefaultAddress = new Address();
            AllAddresses = new List<Address>();
        }
        public int Type { get; set; }
        public Address DefaultAddress { get; set; }
        public IList<Address> AllAddresses { get; set; }
    }
}
