using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class OnlineCustomerController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public OnlineCustomerController(ICustomerModelFactory customerModelFactory,
            IPermissionService permissionService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _customerModelFactory = customerModelFactory;
            _permissionService = permissionService;
        }

        #endregion
        
        #region Methods

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = _customerModelFactory.PrepareOnlineCustomerSearchModel(new OnlineCustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(OnlineCustomerSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedDataTablesJson();

            var model = _customerModelFactory.PrepareOnlineCustomerListModel(searchModel);

            return Json(model);
        }

        #endregion
    }
}