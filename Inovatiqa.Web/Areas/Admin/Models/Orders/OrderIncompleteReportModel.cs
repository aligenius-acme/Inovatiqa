using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class OrderIncompleteReportModel : BaseInovatiqaModel
    {
        #region Properties

        [Display(Name = "Item")]
        public string Item { get; set; }

        [Display(Name = "Total")]
        public string Total { get; set; }

        [Display(Name = "Count")]
        public int Count { get; set; }

        [Display(Name = "view all")]
        public string ViewLink { get; set; }

        #endregion
    }
}