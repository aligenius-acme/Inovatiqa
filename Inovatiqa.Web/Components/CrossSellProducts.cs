using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class CrossSellProductsViewComponent : ViewComponent
    {
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContextService _workContextService;

        public CrossSellProductsViewComponent(IProductModelFactory productModelFactory,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IWorkContextService workContextService)
        {
            _productModelFactory = productModelFactory;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _workContextService = workContextService;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            var products = _productService.GetCrosssellProductsByShoppingCart(cart, InovatiqaDefaults.CrossSellsNumber);

            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products,
                    productThumbPictureSize: productThumbPictureSize, forceRedirectionAfterAddingToCart: true)
                .ToList();

            return View(model);
        }
    }
}