using Inovatiqa.Web.Models.News;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface INewsModelFactory
    {
        HomepageNewsItemsModel PrepareHomepageNewsItemsModel();

        NewsItemModel PrepareNewsItemModel(NewsItemModel model, Inovatiqa.Database.Models.News newsItem, bool prepareComments);
    }
}
