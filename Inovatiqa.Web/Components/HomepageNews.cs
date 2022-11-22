using Inovatiqa.Core;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class HomepageNewsViewComponent : ViewComponent
    {
        private readonly INewsModelFactory _newsModelFactory;

        public HomepageNewsViewComponent(INewsModelFactory newsModelFactory)
        {
            _newsModelFactory = newsModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            if (!InovatiqaDefaults.ShowNewsOnMainPage)
                return Content("");

            var model = _newsModelFactory.PrepareHomepageNewsItemsModel();
            return View(model);
        }
    }
}
