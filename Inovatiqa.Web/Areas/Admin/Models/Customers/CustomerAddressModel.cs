using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerAddressModel : BaseInovatiqaModel
    {
        #region Ctor

        public CustomerAddressModel()
        {
            Address = new AddressModel();
        }

        #endregion

        #region Properties

        public int CustomerId { get; set; }

        public AddressModel Address { get; set; }

        #endregion
    }
}