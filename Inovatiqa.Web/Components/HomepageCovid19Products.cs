using System.Linq;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class HomepageCovid19ProductsViewComponent : ViewComponent
    {
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;

        public HomepageCovid19ProductsViewComponent(IProductModelFactory productModelFactory,
            IProductService productService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            var products = _productService.GetAllCovid19ProductsDisplayedOnHomepage();
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize, prepareProductAttributes: true).ToList();

            return View(model);
        }
    }
}