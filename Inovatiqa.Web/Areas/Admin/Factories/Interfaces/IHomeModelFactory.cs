using Inovatiqa.Web.Areas.Admin.Models.Home;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IHomeModelFactory
    {
        DashboardModel PrepareDashboardModel(DashboardModel model);

        InovatiqaNewsModel PrepareInovatiqaNewsModel();
    }
}