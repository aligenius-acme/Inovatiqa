using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public partial class ManufacturerTemplateService : IManufacturerTemplateService
    {
        #region Fields

        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;

        #endregion

        #region Ctor

        public ManufacturerTemplateService(IRepository<ManufacturerTemplate> manufacturerTemplateRepository)
        {
            _manufacturerTemplateRepository = manufacturerTemplateRepository;
        }

        #endregion

        #region Methods

        public virtual IList<ManufacturerTemplate> GetAllManufacturerTemplates()
        {
            var query = from pt in _manufacturerTemplateRepository.Query()
                        orderby pt.DisplayOrder, pt.Id
                        select pt;

            var templates = query.ToList();

            return templates;
        }
        #endregion
    }
}