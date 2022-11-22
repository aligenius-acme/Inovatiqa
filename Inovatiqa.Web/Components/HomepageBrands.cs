using Inovatiqa.Services.Catalog.Interfaces;
using Inovatiqa.Services.Media.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Inovatiqa.Web.Models.Catalog;
using System.Collections.Generic;
using Inovatiqa.Services.Seo.Interfaces;
using Inovatiqa.Core;

namespace Inovatiqa.Web.Components
{
    public class HomepageBrandsViewComponent : ViewComponent
    {
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;

        public HomepageBrandsViewComponent(
            IManufacturerService manufacturerService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService)
        {
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
        }
        public IViewComponentResult Invoke()
        {
            var model = new List<ManufacturerModel>();
            var manufacturers = _manufacturerService.GetHomePageManufacturers();
            foreach(var manufacturer in manufacturers)
            {
                if(manufacturer.Published && manufacturer.ShowOnHomepage && !manufacturer.Deleted)
                {
                    var picture = _pictureService.GetPictureById(manufacturer.PictureId);
                    //var picture = _pictureService.GetPictureById(1149);
                    var pictureUrl = _pictureService.GetPictureUrl(ref picture);
                    var defaultPicture = _pictureService.GetDefaultPictureUrl();
                    var man = new ManufacturerModel();
                    var seName = _urlRecordService.GetActiveSlug(manufacturer.Id, InovatiqaDefaults.ManufacturerSlugName, InovatiqaDefaults.LanguageId);
                    //man.PictureModel.FullSizeImageUrl = true ? pictureUrl : defaultPicture;
                    man.PictureModel.FullSizeImageUrl = manufacturer.PictureId > 0 ? pictureUrl : defaultPicture;
                    man.SeName = seName;
                    model.Add(man);
                }
            }
            return View(model);
        }
    }
}
