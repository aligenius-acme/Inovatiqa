using Inovatiqa.Core;
using Inovatiqa.Services.Common.Interfaces;
using Inovatiqa.Services.Settings.Interfces;
using Inovatiqa.Services.WorkContext.Interfaces;
using Inovatiqa.Web.Areas.Admin.Factories.Interfaces;
using Inovatiqa.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Inovatiqa.Web.Areas.Admin.Models.Settings;

namespace Inovatiqa.Web.Areas.Admin.Factories
{
    public partial class SettingModelFactory : ISettingModelFactory
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IWorkContextService _workContextService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public SettingModelFactory(ISettingService settingService,
            IWorkContextService workContextService,
            IGenericAttributeService genericAttributeService)
        {
            _settingService = settingService;
            _workContextService = workContextService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Utilities



        #endregion

        #region Methods

        public virtual ProductEditorSettingsModel PrepareProductEditorSettingsModel()
        {
            var storeId = InovatiqaDefaults.StoreId;
            var productEditorSettings = _settingService.LoadSetting<ProductEditorSettings>(storeId);

            var model = productEditorSettings.ToSettingsModel<ProductEditorSettingsModel>();

            return model;
        }

        public virtual SettingModeModel PrepareSettingModeModel(string modeName)
        {
            var customer = _workContextService.CurrentCustomer;

            var model = new SettingModeModel
            {
                ModeName = modeName,
                Enabled = _genericAttributeService.GetAttribute<bool>(customer, modeName, customer.Id)
            };

            return model;
        }

        #endregion
    }
}