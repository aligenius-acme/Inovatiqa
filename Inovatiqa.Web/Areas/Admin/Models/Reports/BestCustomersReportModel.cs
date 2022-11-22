
using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class BestCustomersReportModel : BaseInovatiqaModel
    {
        #region Properties

        public int CustomerId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; }

        [Display(Name = "Order total")]
        public string OrderTotal { get; set; }

        [Display(Name = "Number of orders")]
        public decimal OrderCount { get; set; }
        
        #endregion
    }
}
