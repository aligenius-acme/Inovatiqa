using Microsoft.AspNetCore.Mvc;
using Inovatiqa.Web.Framework.Components;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;

namespace Inovatiqa.Web.Areas.Admin.Components
{
    public class InovatiqaNewsViewComponent : InovatiqaViewComponent
    {
        #region Fields

        private readonly IHomeModelFactory _homeModelFactory;

        #endregion

        #region Ctor

        public InovatiqaNewsViewComponent(IHomeModelFactory homeModelFactory)
        {
            _homeModelFactory = homeModelFactory;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke()
        {
            try
            {
                var model = _homeModelFactory.PrepareInovatiqaNewsModel();

                return View(model);
            }
            catch
            {
                return Content(string.Empty);
            }
        }

        #endregion
    }
}