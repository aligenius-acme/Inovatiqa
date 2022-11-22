using Inovatiqa.Web.Areas.Admin.Models.Common;
using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class OrderAddressModel : BaseInovatiqaModel
    {
        #region Ctor

        public OrderAddressModel()
        {
            Address = new AddressModel();
        }

        #endregion

        #region Properties

        public int OrderId { get; set; }

        public AddressModel Address { get; set; }

        #endregion
    }
}