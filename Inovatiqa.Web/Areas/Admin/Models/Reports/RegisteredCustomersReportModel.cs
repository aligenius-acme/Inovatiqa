using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class RegisteredCustomersReportModel : BaseInovatiqaModel
    {
        #region Properties

        [Display(Name = "Period")]
        public string Period { get; set; }

        [Display(Name = "Count")]
        public int Customers { get; set; }

        #endregion
    }
}
