using Inovatiqa.Core.Caching;
using Inovatiqa.Services.Caching.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using System;
using System.Linq;
using Inovatiqa.Services.Common;
using ICommonModelFactory = Inovatiqa.Web.Areas.Admin.Factories.Interfaces.ICommonModelFactory;
using Inovatiqa.Web.Areas.Admin.Models.Home;
using IOrderModelFactory = Inovatiqa.Web.Areas.Admin.Factories.Interfaces.IOrderModelFactory;
using Inovatiqa.Core;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Cache;
using Inovatiqa.Services.Logging.Interfaces;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class HomeModelFactory : IHomeModelFactory
    {
        #region Fields

        private readonly ICacheKeyService _cacheKeyService;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ILoggerService _loggerService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWorkContextService _workContextService;
        private readonly InovatiqaHttpClient _inovatiqaHttpClient;

        #endregion

        #region Ctor

        public HomeModelFactory(ICacheKeyService cacheKeyService,
            ICommonModelFactory commonModelFactory,
            ILoggerService loggerService,
            IOrderModelFactory orderModelFactory,
            IStaticCacheManager staticCacheManager,
            IWorkContextService workContextService,
            InovatiqaHttpClient inovatiqaHttpClient)
        {
            _cacheKeyService = cacheKeyService;
            _commonModelFactory = commonModelFactory;
            _loggerService = loggerService;
            _orderModelFactory = orderModelFactory;
            _staticCacheManager = staticCacheManager;
            _workContextService = workContextService;
            _inovatiqaHttpClient = inovatiqaHttpClient;
        }

        #endregion

        #region Methods

        public virtual DashboardModel PrepareDashboardModel(DashboardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.IsLoggedInAsVendor = _workContextService.CurrentVendor != null;

            _commonModelFactory.PreparePopularSearchTermSearchModel(model.PopularSearchTerms);
            _orderModelFactory.PrepareBestsellerBriefSearchModel(model.BestsellersByAmount);
            _orderModelFactory.PrepareBestsellerBriefSearchModel(model.BestsellersByQuantity);

            return model;
        }

        public virtual InovatiqaNewsModel PrepareInovatiqaNewsModel()
        {
            var model = new InovatiqaNewsModel
            {
                HideAdvertisements = InovatiqaDefaults.HideAdvertisementsOnAdminArea
            };

            try
            {
                var rssData = _staticCacheManager.Get(_cacheKeyService.PrepareKeyForDefaultCache(InovatiqaModelCacheDefaults.OfficialNewsModelKey), () =>
                {
                    try
                    {
                        return _inovatiqaHttpClient.GetNewsRssAsync().Result;
                    }
                    catch (AggregateException exception)
                    {
                        throw exception.InnerException;
                    }
                });

                for (var i = 0; i < rssData.Items.Count; i++)
                {
                    var item = rssData.Items.ElementAt(i);
                    var newsItem = new InovatiqaNewsDetailsModel
                    {
                        Title = item.TitleText,
                        Summary = item.ContentText,
                        Url = item.Url.OriginalString,
                        PublishDate = item.PublishDate
                    };
                    model.Items.Add(newsItem);

                    if (model.Items.Count > 0)
                        model.HasNewItems = true;
                }
            }
            catch (Exception ex)
            {
                _loggerService.Error("No access to the news.", ex);
            }

            return model;
        }

        #endregion
    }
}