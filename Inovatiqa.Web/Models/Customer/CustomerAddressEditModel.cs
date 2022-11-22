using Inovatiqa.Web.Framework.Models;
using Inovatiqa.Web.Models.Common;

namespace Inovatiqa.Web.Models.Customer
{
    public partial class CustomerAddressEditModel : BaseInovatiqaModel
    {
        public CustomerAddressEditModel()
        {
            Address = new AddressModel();
        }
        
        public AddressModel Address { get; set; }
        public bool IsAdding { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}