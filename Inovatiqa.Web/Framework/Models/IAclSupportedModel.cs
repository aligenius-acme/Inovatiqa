using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inovatiqa.Web.Framework.Models
{
    public partial interface IAclSupportedModel
    {
        #region Properties

        IList<int> SelectedCustomerRoleIds { get; set; }

        IList<SelectListItem> AvailableCustomerRoles { get; set; }

        #endregion
    }
}