using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.Catalog;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IManufacturerModelFactory
    {
        ManufacturerProductListModel PrepareManufacturerProductListModel(ManufacturerProductSearchModel searchModel,
            Manufacturer manufacturer);

        ManufacturerSearchModel PrepareManufacturerSearchModel(ManufacturerSearchModel searchModel);

        ManufacturerListModel PrepareManufacturerListModel(ManufacturerSearchModel searchModel);

        ManufacturerModel PrepareManufacturerModel(ManufacturerModel model,
            Manufacturer manufacturer, bool excludeProperties = false);

    }
}