using Inovatiqa.Core;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class FeaturedManufacturerBlockViewComponent : ViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public FeaturedManufacturerBlockViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            if (!InovatiqaDefaults.FeaturedManufacturerEnabled)
                return Content("");

            var model = _catalogModelFactory.PrepareFeaturedManufacturerModel(InovatiqaDefaults.FeaturedManufacturerNumber);

            return View(model);
        }
    }
}