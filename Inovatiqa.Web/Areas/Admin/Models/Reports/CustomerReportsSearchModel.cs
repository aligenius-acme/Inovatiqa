using Inovatiqa.Web.Framework.Models;

namespace Inovatiqa.Web.Areas.Admin.Models.Reports
{
    public partial class CustomerReportsSearchModel : BaseSearchModel
    {
        #region Ctor

        public CustomerReportsSearchModel()
        {
            BestCustomersByOrderTotal = new BestCustomersReportSearchModel();
            BestCustomersByNumberOfOrders = new BestCustomersReportSearchModel();
            RegisteredCustomers = new RegisteredCustomersReportSearchModel();
        }

        #endregion

        #region Properties

        public BestCustomersReportSearchModel BestCustomersByOrderTotal { get; set; }

        public BestCustomersReportSearchModel BestCustomersByNumberOfOrders { get; set; }

        public RegisteredCustomersReportSearchModel RegisteredCustomers { get; set; }

        #endregion
    }
}