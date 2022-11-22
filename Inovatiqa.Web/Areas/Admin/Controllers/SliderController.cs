using System.Linq;
using Inovatiqa.Core;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Messages.Interfaces;
using Inovatiqa.Services.Security;
using Inovatiqa.Services.Security.Interfaces;
using Inovatiqa.Services.Settings.Interfces;
using Inovatiqa.Web.Areas.Admin.Models.Slider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inovatiqa.Web.Areas.Admin.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class SliderController : BaseAdminController
    {
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;

        public SliderController(INotificationService notificationService,
            IPermissionService permissionService, 
            IPictureService pictureService,
            ISettingService settingService,
             IRazorViewEngine viewEngine) : base(viewEngine)
        {
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _settingService = settingService;
        }

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var nivoSliderSettings = _settingService.LoadSetting<SliderSettings>(InovatiqaDefaults.StoreId);

            var model = new ConfigurationModel
            {
                Picture1Id = nivoSliderSettings.Picture1Id,
                Text1 = nivoSliderSettings.Text1,
                Link1 = nivoSliderSettings.Link1,
                AltText1 = nivoSliderSettings.AltText1,
                Picture2Id = nivoSliderSettings.Picture2Id,
                Text2 = nivoSliderSettings.Text2,
                Link2 = nivoSliderSettings.Link2,
                AltText2 = nivoSliderSettings.AltText2,
                Picture3Id = nivoSliderSettings.Picture3Id,
                Text3 = nivoSliderSettings.Text3,
                Link3 = nivoSliderSettings.Link3,
                AltText3 = nivoSliderSettings.AltText3,
                Picture4Id = nivoSliderSettings.Picture4Id,
                Text4 = nivoSliderSettings.Text4,
                Link4 = nivoSliderSettings.Link4,
                AltText4 = nivoSliderSettings.AltText4,
                Picture5Id = nivoSliderSettings.Picture5Id,
                Text5 = nivoSliderSettings.Text5,
                Link5 = nivoSliderSettings.Link5,
                AltText5 = nivoSliderSettings.AltText5,
                ActiveStoreScopeConfiguration = InovatiqaDefaults.StoreId
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var nivoSliderSettings = _settingService.LoadSetting<SliderSettings>(InovatiqaDefaults.StoreId);

            var previousPictureIds = new[] 
            {
                nivoSliderSettings.Picture1Id,
                nivoSliderSettings.Picture2Id,
                nivoSliderSettings.Picture3Id,
                nivoSliderSettings.Picture4Id,
                nivoSliderSettings.Picture5Id
            };

            nivoSliderSettings.Picture1Id = model.Picture1Id;
            nivoSliderSettings.Text1 = model.Text1;
            nivoSliderSettings.Link1 = model.Link1;
            nivoSliderSettings.AltText1 = model.AltText1;
            nivoSliderSettings.Picture2Id = model.Picture2Id;
            nivoSliderSettings.Text2 = model.Text2;
            nivoSliderSettings.Link2 = model.Link2;
            nivoSliderSettings.AltText2 = model.AltText2;
            nivoSliderSettings.Picture3Id = model.Picture3Id;
            nivoSliderSettings.Text3 = model.Text3;
            nivoSliderSettings.Link3 = model.Link3;
            nivoSliderSettings.AltText3 = model.AltText3;
            nivoSliderSettings.Picture4Id = model.Picture4Id;
            nivoSliderSettings.Text4 = model.Text4;
            nivoSliderSettings.Link4 = model.Link4;
            nivoSliderSettings.AltText4 = model.AltText4;
            nivoSliderSettings.Picture5Id = model.Picture5Id;
            nivoSliderSettings.Text5 = model.Text5;
            nivoSliderSettings.Link5 = model.Link5;
            nivoSliderSettings.AltText5 = model.AltText5;

            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Picture1Id, model.Picture1Id_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Text1, model.Text1_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Link1, model.Link1_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.AltText1, model.AltText1_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Picture2Id, model.Picture2Id_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Text2, model.Text2_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Link2, model.Link2_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.AltText2, model.AltText2_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Picture3Id, model.Picture3Id_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Text3, model.Text3_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Link3, model.Link3_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.AltText3, model.AltText3_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Picture4Id, model.Picture4Id_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Text4, model.Text4_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Link4, model.Link4_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.AltText4, model.AltText4_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Picture5Id, model.Picture5Id_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Text5, model.Text5_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.Link5, model.Link5_OverrideForStore, InovatiqaDefaults.StoreId, false);
            _settingService.SaveSettingOverridablePerStore(nivoSliderSettings, x => x.AltText5, model.AltText5_OverrideForStore, InovatiqaDefaults.StoreId, false);
            
            //get current picture identifiers
            var currentPictureIds = new[]
            {
                nivoSliderSettings.Picture1Id,
                nivoSliderSettings.Picture2Id,
                nivoSliderSettings.Picture3Id,
                nivoSliderSettings.Picture4Id,
                nivoSliderSettings.Picture5Id
            };

            //delete an old picture (if deleted or updated)
            foreach (var pictureId in previousPictureIds.Except(currentPictureIds))
            { 
                var previousPicture = _pictureService.GetPictureById(pictureId);
                if (previousPicture != null)
                    _pictureService.DeletePicture(previousPicture);
            }

            _notificationService.SuccessNotification("Updated successfully.");
            return Configure();
        }
    }
}