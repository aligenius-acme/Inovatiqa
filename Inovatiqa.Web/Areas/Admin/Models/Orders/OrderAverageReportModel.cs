using Inovatiqa.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Inovatiqa.Web.Areas.Admin.Models.Orders
{
    public partial class OrderAverageReportModel : BaseInovatiqaModel
    {
        #region Properties

        [Display(Name = "Order Status")]
        public string OrderStatus { get; set; }

        [Display(Name = "Today")]
        public string SumTodayOrders { get; set; }

        [Display(Name = "This Week")]
        public string SumThisWeekOrders { get; set; }

        [Display(Name = "This Month")]
        public string SumThisMonthOrders { get; set; }

        [Display(Name = "This Year")]
        public string SumThisYearOrders { get; set; }

        [Display(Name = "All time")]
        public string SumAllTimeOrders { get; set; }

        #endregion
    }
}