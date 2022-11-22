using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Shipping
{
    public partial class DatesRangesSearchModel : BaseSearchModel
    {
        #region Ctor

        public DatesRangesSearchModel()
        {
            DeliveryDateSearchModel = new DeliveryDateSearchModel();
            ProductAvailabilityRangeSearchModel = new ProductAvailabilityRangeSearchModel();
        }

        #endregion

        #region Properties

        public DeliveryDateSearchModel DeliveryDateSearchModel { get; set; }

        public ProductAvailabilityRangeSearchModel ProductAvailabilityRangeSearchModel { get; set; }

        #endregion
    }
}