using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Customers
{
    public partial class CustomerRewardPointsSearchModel : BaseSearchModel
    {
        #region Properties

        public int CustomerId { get; set; }
        
        #endregion
    }
}