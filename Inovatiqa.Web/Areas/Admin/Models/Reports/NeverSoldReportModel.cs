using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class NeverSoldReportModel : BaseInovatiqaModel
    {
        #region Properties

        public int ProductId { get; set; }

        [Display(Name = "Name")]
        public string ProductName { get; set; }

        #endregion
    }
}
