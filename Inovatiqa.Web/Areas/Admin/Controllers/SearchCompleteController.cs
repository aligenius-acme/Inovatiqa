using System.Linq;
using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public class SearchCompleteController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IWorkContextService _workContextService;

        #endregion

        #region Ctor

        public SearchCompleteController(
            IPermissionService permissionService,
            IProductService productService,
            IWorkContextService workContextService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _permissionService = permissionService;
            _productService = productService;
            _workContextService = workContextService;
        }

        #endregion

        #region Methods

        public virtual IActionResult SearchAutoComplete(string term)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return Content(string.Empty);

            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);

            var vendorId = 0;

            var vendor = _workContextService.CurrentVendor;

            if (vendor != null)
            {
                vendorId = vendor.Id;
            }

            const int productNumber = 15;
            var products = _productService.SearchProducts(
                vendorId: vendorId,
                keywords: term,
                pageSize: productNumber,
                showHidden: true);

            var result = (from p in products
                            select new
                            {
                                label = p.Name,
                                productid = p.Id
                            }).ToList();

            return Json(result);
        }

        #endregion
    }
}