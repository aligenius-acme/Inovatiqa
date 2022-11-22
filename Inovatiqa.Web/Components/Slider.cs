using Inovatiqa.Core;
using Inovatiqa.Core.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Inovatiqa.Services.Settings.Interfces;
using Inovatiqa.Web.Models.Slider;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    [ViewComponent(Name = "Slider")]
    public class Slider : ViewComponent
    {
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;

        public Slider( 
            IPictureService pictureService,
            IWebHelper webHelper,
            ISettingService settingService)
        {
            _pictureService = pictureService;
            _webHelper = webHelper;
            _settingService = settingService;
        }

        public IViewComponentResult Invoke()
        {
            //var model = new PublicInfoModel();

            //var settings = _settingService.LoadSliderSettings(InovatiqaDefaults.SliderTypeId, InovatiqaDefaults.StoreId);

            var settings = _settingService.LoadSetting<SliderSettings>(InovatiqaDefaults.StoreId);

            var model = new PublicInfoModel
            {
                Picture1Url = GetPictureUrl(settings.Picture1Id),
                Text1 = settings.Text1,
                Link1 = settings.Link1,
                AltText1 = settings.AltText1,

                Picture2Url = GetPictureUrl(settings.Picture2Id),
                Text2 = settings.Text2,
                Link2 = settings.Link2,
                AltText2 = settings.AltText2,

                Picture3Url = GetPictureUrl(settings.Picture3Id),
                Text3 = settings.Text3,
                Link3 = settings.Link3,
                AltText3 = settings.AltText3,

                Picture4Url = GetPictureUrl(settings.Picture4Id),
                Text4 = settings.Text4,
                Link4 = settings.Link4,
                AltText4 = settings.AltText4,

                Picture5Url = GetPictureUrl(settings.Picture5Id),
                Text5 = settings.Text5,
                Link5 = settings.Link5,
                AltText5 = settings.AltText5
            };

            if (string.IsNullOrEmpty(model.Picture1Url) && string.IsNullOrEmpty(model.Picture2Url) &&
                string.IsNullOrEmpty(model.Picture3Url) && string.IsNullOrEmpty(model.Picture4Url) &&
                string.IsNullOrEmpty(model.Picture5Url))
                //no pictures uploaded
                return Content("");

            return View(model);

            ////////////if (settings != null)
            ////////////{
            ////////////    foreach (var setting in settings)
            ////////////    {
            ////////////        if (setting.Name == "nivoslidersettings.picture1id")
            ////////////            model.Picture1Url = GetPictureUrl(int.Parse(setting.Value));
            ////////////        else if (setting.Name == "nivoslidersettings.text1")
            ////////////            model.Text1 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.link1")
            ////////////            model.Link1 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.alttext1")
            ////////////            model.AltText1 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.picture2id")
            ////////////            model.Picture2Url = GetPictureUrl(int.Parse(setting.Value));
            ////////////        else if (setting.Name == "nivoslidersettings.text2")
            ////////////            model.Text2 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.link2")
            ////////////            model.Link2 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.alttext2")
            ////////////            model.AltText2 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.picture3id")
            ////////////            model.Picture3Url = GetPictureUrl(int.Parse(setting.Value));
            ////////////        else if (setting.Name == "nivoslidersettings.text3")
            ////////////            model.Text3 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.link3")
            ////////////            model.Link3 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.alttext3")
            ////////////            model.AltText3 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.picture4id")
            ////////////            model.Picture4Url = GetPictureUrl(int.Parse(setting.Value));
            ////////////        else if (setting.Name == "nivoslidersettings.text4")
            ////////////            model.Text4 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.link4")
            ////////////            model.Link4 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.alttext4")
            ////////////            model.AltText4 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.picture5id")
            ////////////            model.Picture5Url = GetPictureUrl(int.Parse(setting.Value));
            ////////////        else if (setting.Name == "nivoslidersettings.text5")
            ////////////            model.Text5 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.link5")
            ////////////            model.Link5 = setting.Value;
            ////////////        else if (setting.Name == "nivoslidersettings.alttext5")
            ////////////            model.AltText5 = setting.Value;
            ////////////    }
            ////////////}

            ////////////if (string.IsNullOrEmpty(model.Picture1Url) && string.IsNullOrEmpty(model.Picture2Url) &&
            ////////////    string.IsNullOrEmpty(model.Picture3Url) && string.IsNullOrEmpty(model.Picture4Url) &&
            ////////////    string.IsNullOrEmpty(model.Picture5Url))
            ////////////    return Content("");

            ////////////return View(model);
        }

        protected string GetPictureUrl(int pictureId)
        {
            var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false) ?? "";
            return url;
        }
    }
}
