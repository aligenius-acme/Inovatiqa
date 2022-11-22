using Inovatiqa.Web.Areas.Admin.Models.Settings;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface ISettingModelFactory
    {
        ProductEditorSettingsModel PrepareProductEditorSettingsModel();

        SettingModeModel PrepareSettingModeModel(string modeName);
    }
}