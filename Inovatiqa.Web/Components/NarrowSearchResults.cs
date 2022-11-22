using Inovatiqa.Web.Factories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class NarrowSearchResultsViewComponent : ViewComponent
    {
        #region Fields

        private readonly ICatalogModelFactory _catalogModelFactory;

        #endregion

        #region Ctor

        public NarrowSearchResultsViewComponent(ICatalogModelFactory catalogModelFactory)
        {
            _catalogModelFactory = catalogModelFactory;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(int currentCategoryId, int currentProductId)
        {
            var model = _catalogModelFactory.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);
            return View(model);
        }

        #endregion
    }
}
