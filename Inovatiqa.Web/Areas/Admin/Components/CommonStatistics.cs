using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Areas.Admin.Components
{
    public class CommonStatisticsViewComponent : InovatiqaViewComponent
    {
        #region Fields

        private readonly ICommonModelFactory _commonModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public CommonStatisticsViewComponent(ICommonModelFactory commonModelFactory,
            IPermissionService permissionService,
            IWorkContextService workContextService)
        {
            _commonModelFactory = commonModelFactory;
            _permissionService = permissionService;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageOrders) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests) ||
                !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
            {
                return Content(string.Empty);
            }

            if (_workContextService.CurrentVendor != null)
                return Content(string.Empty);

            var model = _commonModelFactory.PrepareCommonStatisticsModel();

            return View(model);
        }

        #endregion
    }
}