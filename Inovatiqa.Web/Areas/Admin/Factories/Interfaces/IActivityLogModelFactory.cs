using Inovatiqa.Web.Areas.Admin.Models.Logging;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IActivityLogModelFactory
    {
        ActivityLogSearchModel PrepareActivityLogSearchModel(ActivityLogSearchModel searchModel);

        ActivityLogTypeSearchModel PrepareActivityLogTypeSearchModel(ActivityLogTypeSearchModel searchModel);

        ActivityLogListModel PrepareActivityLogListModel(ActivityLogSearchModel searchModel);
    }
}