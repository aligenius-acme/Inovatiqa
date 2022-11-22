using Microsoft.AspNetCore.Mvc;

namespace Inovatiqa.Web.Components
{
    public class ProductsAlsoPurchasedViewComponent : ViewComponent
    {
        #region Fields


        #endregion

        #region Ctor

        public ProductsAlsoPurchasedViewComponent()
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