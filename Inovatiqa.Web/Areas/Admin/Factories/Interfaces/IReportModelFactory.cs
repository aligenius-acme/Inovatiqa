using Inovatiqa.Web.Areas.Admin.Models.Reports;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IReportModelFactory
    {
        #region LowStockProduct

        LowStockProductSearchModel PrepareLowStockProductSearchModel(LowStockProductSearchModel searchModel);

        LowStockProductListModel PrepareLowStockProductListModel(LowStockProductSearchModel searchModel);

        #endregion

        #region Bestseller

        BestsellerSearchModel PrepareBestsellerSearchModel(BestsellerSearchModel searchModel);

        BestsellerListModel PrepareBestsellerListModel(BestsellerSearchModel searchModel);

        string GetBestsellerTotalAmount(BestsellerSearchModel searchModel);

        #endregion

        #region NeverSold

        NeverSoldReportSearchModel PrepareNeverSoldSearchModel(NeverSoldReportSearchModel searchModel);

        NeverSoldReportListModel PrepareNeverSoldListModel(NeverSoldReportSearchModel searchModel);

        #endregion

        #region Country sales

        CountryReportSearchModel PrepareCountrySalesSearchModel(CountryReportSearchModel searchModel);

        CountryReportListModel PrepareCountrySalesListModel(CountryReportSearchModel searchModel);

        #endregion

        #region Customer reports

        CustomerReportsSearchModel PrepareCustomerReportsSearchModel(CustomerReportsSearchModel searchModel);

        BestCustomersReportListModel PrepareBestCustomersReportListModel(BestCustomersReportSearchModel searchModel);

        RegisteredCustomersReportListModel PrepareRegisteredCustomersReportListModel(RegisteredCustomersReportSearchModel searchModel);

        #endregion
    }
}
