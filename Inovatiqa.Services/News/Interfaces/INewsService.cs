using Inovatiqa.Core.Interfaces;

namespace Inovatiqa.Services.News.Interfaces
{
    public partial interface INewsService
    {
        #region News

        IPagedList<Inovatiqa.Database.Models.News> GetAllNews(int languageId = 0, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null);

        Inovatiqa.Database.Models.News GetNewsById(int newsId);

        #endregion

        #region News comments



        #endregion
    }
}