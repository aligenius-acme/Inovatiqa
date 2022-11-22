using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class MostViewedProductsBlockViewComponent : ViewComponent
    {
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;

        public MostViewedProductsBlockViewComponent(IProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize, bool? preparePriceModel)
        {
            if (!InovatiqaDefaults.MostViewedProductsEnabled)
                return Content("");

            var preparePictureModel = productThumbPictureSize.HasValue;

            var products = _productService.GetMostViewedProducts(InovatiqaDefaults.MostViewedProductsNumber);

            if (!products.Any())
                return Content("");

            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products,
                preparePriceModel.GetValueOrDefault(),
                preparePictureModel,
                productThumbPictureSize));

            return View(model);
        }
    }
}