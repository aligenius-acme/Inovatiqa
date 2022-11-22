using Inovatiqa.Web.Framework.Models;
using System.Collections.Generic;

namespace Inovatiqa.Web.Areas.Admin.Models.Logging
{
    public partial class ActivityLogTypeSearchModel : BaseSearchModel
    {
        #region Properties       

        public IList<ActivityLogTypeModel> ActivityLogTypeListModel { get; set; }

        #endregion
    }
}