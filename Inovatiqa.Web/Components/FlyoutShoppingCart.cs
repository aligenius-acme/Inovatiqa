using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class FlyoutShoppingCartViewComponent : ViewComponent
    {
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;

        public FlyoutShoppingCartViewComponent(IPermissionService permissionService,
            IShoppingCartModelFactory shoppingCartModelFactory)
        {
            _permissionService = permissionService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return Content("");
            var model = _shoppingCartModelFactory.PrepareMiniShoppingCartModel();
            return View(model);
        }
    }
}
