using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class HeaderLinksViewComponent : ViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;
        //Naveed Modifications
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IWorkContextService _workContextService;
        public HeaderLinksViewComponent(ICommonModelFactory commonModelFactory,
             IShoppingCartModelFactory shoppingCartModelFactory,
             ICustomerService customerService,
             IWorkContextService workContextService)
        {
            _commonModelFactory = commonModelFactory;
            //Naveed Modifications
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _customerService = customerService;
            _workContextService = workContextService;
        }

        public IViewComponentResult Invoke()
        {
            TempData["name"] = "Ali Ahmad";
            var model = _commonModelFactory.PrepareHeaderLinksModel();
            //Naveed Modifications
            model.miniShoppingCartModel = _shoppingCartModelFactory.PrepareMiniShoppingCartModel();
            var customer = _workContextService.CurrentCustomer;
            TempData["IsB2B"] = _customerService.IsB2B(customer);
            return View(model);
        }
    }
}