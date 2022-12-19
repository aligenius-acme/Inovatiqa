using Inovatiqa.Web.Factories.Interfaces;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Inovatiqa.Web.Models.Catalog;

namespace Inovatiqa.Web.Components
{
    public class ManufacturerNavigationViewComponent : ViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;

        public ManufacturerNavigationViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        public IViewComponentResult Invoke(int currentManufacturerId, List<ManufacturerBriefInfoModel> allManufacturers)
        {
            //var model = _catalogModelFactory.PrepareManufacturerNavigationModel(currentManufacturerId, selectedManufacturers);
            var model = new ManufacturerNavigationModel();
            model.Manufacturers = allManufacturers;
            if (model.Manufacturers == null || !model.Manufacturers.Any())
                return Content("");

            return View(model);
        }
    }
}
