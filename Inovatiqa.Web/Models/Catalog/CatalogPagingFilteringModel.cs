using System.Collections.Generic;
using Inovatiqa.Web.Paging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Models.Catalog
{
    public partial class CatalogPagingFilteringModel : BasePageableModel
    {
        #region Ctor

        public CatalogPagingFilteringModel()
        {
            PageSizeOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        public IList<SelectListItem> PageSizeOptions { get; set; }

        #endregion
    }
}