using System;
using Inovatiqa.Services.Customers.Interfaces;
using Inovatiqa.Services.Orders.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.ShoppingCart;
using Inovatiqa.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class ShoppingCartController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        #endregion

        #region Ctor

        public ShoppingCartController(ICustomerService customerService,
            IPermissionService permissionService,
            IShoppingCartService shoppingCartService,
            IShoppingCartModelFactory shoppingCartModelFactory,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _customerService = customerService;
            _permissionService = permissionService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
        }

        #endregion

        #region Methods

        public virtual IActionResult CurrentCarts()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrentCarts))
                return AccessDeniedView();

            var model = _shoppingCartModelFactory.PrepareShoppingCartSearchModel(new ShoppingCartSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult CurrentCarts(ShoppingCartSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrentCarts))
                return AccessDeniedDataTablesJson();

            var model = _shoppingCartModelFactory.PrepareShoppingCartListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult GetCartDetails(ShoppingCartItemSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrentCarts))
                return AccessDeniedDataTablesJson();

            var customer = _customerService.GetCustomerById(searchModel.CustomerId)
                ?? throw new ArgumentException("No customer found with the specified id");

            var model = _shoppingCartModelFactory.PrepareShoppingCartItemListModel(searchModel, customer);

            return Json(model);
        }

        [HttpPost]
        public virtual IActionResult DeleteItem(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrentCarts))
                return AccessDeniedDataTablesJson();

            _shoppingCartService.DeleteShoppingCartItem(id);

            return new NullJsonResult();
        }

        #endregion
    }
}