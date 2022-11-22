using Inovatiqa.Core;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.ShoppingCart;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class OrderSummaryCheckoutViewComponent : ViewComponent
    {
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContextService _workContextService;

        public OrderSummaryCheckoutViewComponent(IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IWorkContextService workContextService)
        {
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _workContextService = workContextService;
        }

        public IViewComponentResult Invoke(bool? prepareAndDisplayOrderReviewData, ShoppingCartModel overriddenModel)
        {
            if (overriddenModel != null)
                return View(overriddenModel);

            var cart = _shoppingCartService.GetShoppingCart(_workContextService.CurrentCustomer, (int)ShoppingCartType.ShoppingCart, InovatiqaDefaults.StoreId);

            var model = new ShoppingCartModel();
            model = _shoppingCartModelFactory.PrepareShoppingCartModel(model, cart,
                isEditable: false,
                prepareAndDisplayOrderReviewData: prepareAndDisplayOrderReviewData.GetValueOrDefault());
            return View(model);
        }
    }
}
