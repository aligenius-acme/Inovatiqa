using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class TierPriceSearchModel : BaseSearchModel
    {
        #region Properties

        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int CustomerId { get; set; }

        #endregion
    }
}