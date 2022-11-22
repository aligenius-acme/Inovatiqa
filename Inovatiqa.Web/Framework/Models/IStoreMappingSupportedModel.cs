using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Framework.Models
{
    public partial interface IStoreMappingSupportedModel
    {
        #region Properties

        IList<int> SelectedStoreIds { get; set; }

        IList<SelectListItem> AvailableStores { get; set; }

        #endregion
    }
}