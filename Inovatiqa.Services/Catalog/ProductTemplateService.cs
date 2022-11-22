using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public partial class ProductTemplateService : IProductTemplateService
    {
        #region Fields

        private readonly IRepository<ProductTemplate> _productTemplateRepository;

        #endregion

        #region Ctor

        public ProductTemplateService(IRepository<ProductTemplate> productTemplateRepository)
        {
            _productTemplateRepository = productTemplateRepository;
        }

        #endregion

        #region Methods
    
        public virtual IList<ProductTemplate> GetAllProductTemplates()
        {
            var query = from pt in _productTemplateRepository.Query()
                        orderby pt.DisplayOrder, pt.Id
                        select pt;

            var templates = query.ToList();

            return templates;
        }

        #endregion
    }
}