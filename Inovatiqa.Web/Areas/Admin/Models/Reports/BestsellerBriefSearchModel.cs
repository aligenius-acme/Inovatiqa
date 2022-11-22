using Inovatiqa.Services.Orders;
using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class BestsellerBriefSearchModel : BaseSearchModel
    {
        #region Properties

        public OrderByEnum OrderBy { get; set; }

        #endregion
    }
}