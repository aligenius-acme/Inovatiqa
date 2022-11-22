using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;

namespace Inovatiqa.Web.Areas.Admin.Models.Catalog
{
    public partial class AddProductToManufacturerModel : BaseInovatiqaModel
    {
        #region Ctor

        public AddProductToManufacturerModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        public int ManufacturerId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}