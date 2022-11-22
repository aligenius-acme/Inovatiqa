using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Database.Interfaces;
using Inovatiqa.Services.News.Interfaces;
using System;
using System.Linq;

namespace Inovatiqa.Services.News
{
    public partial class NewsService : INewsService
    {
        #region Fields

        private readonly IRepository<Inovatiqa.Database.Models.News> _newsItemRepository;

        #endregion

        #region Ctor

        public NewsService(IRepository<Inovatiqa.Database.Models.News> newsItemRepository)
        {
            _newsItemRepository = newsItemRepository;
        }

        #endregion

        #region Methods


        #region News

        public virtual IPagedList<Inovatiqa.Database.Models.News> GetAllNews(int languageId = 0, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string title = null)
        {
            var query = _newsItemRepository.Query();
            if (languageId > 0)
                query = query.Where(n => languageId == n.LanguageId);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(n => n.Title.Contains(title));

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(n => n.Published);
                query = query.Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow);
                query = query.Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow);
            }

            query = query.OrderByDescending(n => n.StartDateUtc ?? n.CreatedOnUtc);

            var news = new PagedList<Inovatiqa.Database.Models.News>(query, pageIndex, pageSize);

            return news;
        }

        public virtual Inovatiqa.Database.Models.News GetNewsById(int newsId)
        {
            if (newsId == 0)
                return null;

            return _newsItemRepository.GetById(newsId);
        }

        #endregion

        #region News comments


        #endregion

        #endregion
    }
}