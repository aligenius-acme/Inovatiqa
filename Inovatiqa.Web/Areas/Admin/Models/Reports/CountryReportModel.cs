using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class CountryReportModel : BaseInovatiqaModel
    {
        #region Properties

        [Display(Name = "Country")]
        public string CountryName { get; set; }

        [Display(Name = "Number of orders")]
        public int TotalOrders { get; set; }

        [Display(Name = "Order total")]
        public string SumOrders { get; set; }

        #endregion
    }
}
