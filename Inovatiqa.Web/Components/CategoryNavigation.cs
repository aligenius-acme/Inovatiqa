using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Inovatiqa.Web.Components
{
    public class CategoryNavigationViewComponent : ViewComponent
    {
        #region Fields

        private readonly ICatalogModelFactory _catalogModelFactory;

        #endregion

        #region Ctor

        public CategoryNavigationViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(int currentCategoryId, int currentProductId, List<KeyValuePair<string, int>> CategoryCount)
        {
            var model = _catalogModelFactory.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            model.CategoryCount = CategoryCount;
            return View(model);
        }

        #endregion
    }
}
