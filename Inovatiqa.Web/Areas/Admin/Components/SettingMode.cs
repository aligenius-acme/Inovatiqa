using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Framework.Components;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Areas.Admin.Components
{
    public class SettingModeViewComponent : InovatiqaViewComponent
    {
        #region Fields

        private readonly ISettingModelFactory _settingModelFactory;

        #endregion

        #region Ctor

        public SettingModeViewComponent(ISettingModelFactory settingModelFactory)
        {
            _settingModelFactory = settingModelFactory;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(string modeName = "settings-advanced-mode")
        {
            var model = _settingModelFactory.PrepareSettingModeModel(modeName);

            return View(model);
        }

        #endregion
    }
}