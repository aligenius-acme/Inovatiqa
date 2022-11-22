using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace Inovatiqa.Web.Components
{
    public class SearchBoxCartViewComponent : ViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public SearchBoxCartViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            var model = _catalogModelFactory.PrepareSearchBoxModel();
            return View(model);
        }
    }
}
