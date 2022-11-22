using Inovatiqa.Core;
using Inovatiqa.Services.Helpers.Interfaces;
using Inovatiqa.Services.News.Interfaces;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.News;
using System;
using System.Linq;

namespace Inovatiqa.Web.Factories
{
    public partial class NewsModelFactory : INewsModelFactory
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IDateTimeHelperService _dateTimeHelper;

        #endregion

        #region Ctor

        public NewsModelFactory(INewsService newsService,
            IUrlRecordService urlRecordService,
            IDateTimeHelperService dateTimeHelper)
        {
            _newsService = newsService;
            _urlRecordService = urlRecordService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual HomepageNewsItemsModel PrepareHomepageNewsItemsModel()
        {
            var newsItems = _newsService.GetAllNews(InovatiqaDefaults.LanguageId, InovatiqaDefaults.StoreId, 0, InovatiqaDefaults.MainPageNewsCount);
            return new HomepageNewsItemsModel
            {
                WorkingLanguageId = InovatiqaDefaults.LanguageId,
                NewsItems = newsItems
                    .Select(x =>
                    {
                        var newsModel = new NewsItemModel();
                        PrepareNewsItemModel(newsModel, x, false);
                        return newsModel;
                    }).ToList()
            };
        }

        public virtual NewsItemModel PrepareNewsItemModel(NewsItemModel model, Inovatiqa.Database.Models.News newsItem, bool prepareComments)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (newsItem == null)
                throw new ArgumentNullException(nameof(newsItem));

            model.Id = newsItem.Id;
            model.MetaTitle = newsItem.MetaTitle;
            model.MetaDescription = newsItem.MetaDescription;
            model.MetaKeywords = newsItem.MetaKeywords;
            model.SeName = _urlRecordService.GetActiveSlug(newsItem.Id, InovatiqaDefaults.NewsSlugName, InovatiqaDefaults.LanguageId);
            model.Title = newsItem.Title;
            model.Short = newsItem.Short;
            model.Full = newsItem.Full;
            model.AllowComments = newsItem.AllowComments;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.StartDateUtc ?? newsItem.CreatedOnUtc, DateTimeKind.Utc);

            return model;
        }

        #endregion
    }
}