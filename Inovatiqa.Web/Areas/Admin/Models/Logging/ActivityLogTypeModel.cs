using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Logging
{
    public partial class ActivityLogTypeModel : BaseInovatiqaEntityModel
    {
        #region Properties

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Is Enabled")]
        public bool Enabled { get; set; }

        #endregion
    }
}
