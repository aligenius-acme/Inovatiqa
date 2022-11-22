using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class ProductAttributeValueSearchModel : BaseSearchModel
    {
        #region Properties

        public int ProductAttributeMappingId { get; set; }

        #endregion
    }
}