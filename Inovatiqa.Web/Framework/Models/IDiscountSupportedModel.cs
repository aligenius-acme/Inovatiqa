using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Framework.Models
{
    public partial interface IDiscountSupportedModel
    {
        #region Properties

        IList<int> SelectedDiscountIds { get; set; }

        IList<SelectListItem> AvailableDiscounts { get; set; }

        #endregion
    }
}