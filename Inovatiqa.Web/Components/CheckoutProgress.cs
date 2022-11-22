using Inovatiqa.Web.Factories.Interfaces;
using Inovatiqa.Web.Models.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class CheckoutProgressViewComponent:ViewComponent
    {
        private readonly ICheckoutModelFactory _checkoutModelFactory;

        public CheckoutProgressViewComponent(ICheckoutModelFactory checkoutModelFactory)
        {
            _checkoutModelFactory = checkoutModelFactory;
        }

        public IViewComponentResult Invoke(CheckoutProgressStep step)
        {
            var model = _checkoutModelFactory.PrepareCheckoutProgressModel(step);
            return View(model);
        }
    }
}
