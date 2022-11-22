using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class RelatedProductsViewComponent : ViewComponent
    {
        #region Fields


        #endregion

        #region Ctor

        public RelatedProductsViewComponent()
        {
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(int currentCategoryId, int currentProductId)
        {
            return View();
        }

        #endregion
    }
}