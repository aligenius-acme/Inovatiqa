using Inovatiqa.Services.News.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Controllers
{
    public partial class NewsController : BasePublicController
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly INewsModelFactory _newsModelFactory;

        #endregion

        #region Ctor

        public NewsController(INewsService newsService,
            INewsModelFactory newsModelFactory,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _newsService = newsService;
            _newsModelFactory = newsModelFactory;
        }

        #endregion

        #region Methods

        public virtual IActionResult NewsItem(int newsItemId)
        {
            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null)
                return InvokeHttp404();

            var model = new NewsItemModel();
            model = _newsModelFactory.PrepareNewsItemModel(model, newsItem, true);

            return View(model);
        }

        #endregion
    }
}