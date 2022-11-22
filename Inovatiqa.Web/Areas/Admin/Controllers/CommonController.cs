using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class CommonController : BaseAdminController
    {
        #region Const

        private const string EXPORT_IMPORT_PATH = @"files\exportimport";

        #endregion

        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICommonModelFactory _commonModelFactory;

        #endregion

        #region Ctor

        public CommonController(IPermissionService permissionService,
            ICommonModelFactory commonModelFactory,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _commonModelFactory = commonModelFactory;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        [HttpPost]
        public virtual IActionResult PopularSearchTermsReport(PopularSearchTermSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var model = _commonModelFactory.PreparePopularSearchTermListModel(searchModel);

            return Json(model);
        }

        #endregion
    }
}