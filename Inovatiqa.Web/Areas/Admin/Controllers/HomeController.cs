using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Models.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    public partial class HomeController : BaseAdminController
    {
        #region Fields

        private readonly IHomeModelFactory _homeModelFactory;

        #endregion

        #region Ctor

        public HomeController(IHomeModelFactory homeModelFactory,
            IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _homeModelFactory = homeModelFactory;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            var model = _homeModelFactory.PrepareDashboardModel(new DashboardModel());

            return View(model);
        }
        #endregion
    }
}