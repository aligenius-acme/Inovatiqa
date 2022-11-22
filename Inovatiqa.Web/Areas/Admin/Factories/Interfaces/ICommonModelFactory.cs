using Inovatiqa.Web.Areas.Admin.Models.Common;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface ICommonModelFactory
    {
        PopularSearchTermSearchModel PreparePopularSearchTermSearchModel(PopularSearchTermSearchModel searchModel);

        CommonStatisticsModel PrepareCommonStatisticsModel();

        PopularSearchTermListModel PreparePopularSearchTermListModel(PopularSearchTermSearchModel searchModel);
    }
}