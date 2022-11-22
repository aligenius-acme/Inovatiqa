using Inovatiqa.Database.Interfaces;
using Inovatiqa.Database.Models;
using Inovatiqa.Services.Catalog.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Inovatiqa.Services.Catalog
{
    public partial class CategoryTemplateService : ICategoryTemplateService
    {
        #region Fields

        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;

        #endregion

        #region Ctor

        public CategoryTemplateService(IRepository<CategoryTemplate> categoryTemplateRepository)
        {
            _categoryTemplateRepository = categoryTemplateRepository;
        }

        #endregion

        #region Methods

        
        public virtual IList<CategoryTemplate> GetAllCategoryTemplates()
        {
            var query = from pt in _categoryTemplateRepository.Query()
                        orderby pt.DisplayOrder, pt.Id
                        select pt;

            var templates = query.ToList();

            return templates;
        }

        #endregion
    }
}