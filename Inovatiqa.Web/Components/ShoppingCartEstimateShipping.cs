using Inovatiqa.Core;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class ShoppingCartEstimateShippingViewComponent : ViewComponent
    {
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContextService _workContextService;

        public ShoppingCartEstimateShippingViewComponent(IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IWorkContextService workContextService)
        {
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _workContextService = workContextService;
        }

        public IViewComponentResult Invoke(bool? prepareAndDisplayOrderReviewData)
        {
            if (!InovatiqaDefaults.EstimateShippingCartPageEnabled)
                return Content("");

            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            var model = _shoppingCartModelFactory.PrepareEstimateShippingModel(cart);
            if (!model.Enabled)
                return Content("");

            return View(model);
        }
    }
}
