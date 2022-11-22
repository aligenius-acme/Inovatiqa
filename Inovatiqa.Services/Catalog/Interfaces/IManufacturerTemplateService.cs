using Inovatiqa.Database.Models;
using System.Collections.Generic;

namespace Inovatiqa.Services.Catalog.Interfaces
{
    public partial interface IManufacturerTemplateService
    {
        IList<ManufacturerTemplate> GetAllManufacturerTemplates();
    }
}
