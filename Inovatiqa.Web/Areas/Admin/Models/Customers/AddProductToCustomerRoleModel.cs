using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class AddProductToCustomerRoleModel : BaseInovatiqaEntityModel
    {
        #region Properties

        public int AssociatedToProductId { get; set; }

        #endregion
    }
}