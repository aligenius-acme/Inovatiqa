using System.Collections.Generic;
using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Catalog;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class RecentlyViewedProductsBlockViewComponent : ViewComponent
    {
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;

        public RecentlyViewedProductsBlockViewComponent(IProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize, bool? preparePriceModel)
        {
            if (!InovatiqaDefaults.RecentlyViewedProductsEnabled)
                return Content("");

            var preparePictureModel = productThumbPictureSize.HasValue;
            var products = _recentlyViewedProductsService.GetRecentlyViewedProducts(InovatiqaDefaults.RecentlyViewedProductsNumber);

            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

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