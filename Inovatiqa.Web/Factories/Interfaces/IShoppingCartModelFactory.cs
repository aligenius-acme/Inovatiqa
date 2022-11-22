using Inovatiqa.Database.Models;
using Inovatiqa.Web.Models.Media;
using Inovatiqa.Web.Models.ShoppingCart;
using System.Collections.Generic;

namespace Inovatiqa.Web.Factories.Interfaces
{
    public partial interface IShoppingCartModelFactory
    {
        EstimateShippingModel PrepareEstimateShippingModel(IList<ShoppingCartItem> cart, bool setEstimateShippingDefaultAddress = true);
        
        MiniShoppingCartModel PrepareMiniShoppingCartModel();

        PictureModel PrepareCartItemPictureModel(ShoppingCartItem sci, int pictureSize, bool showDefaultPicture, string productName);

        ShoppingCartModel PrepareShoppingCartModel(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareAndDisplayOrderReviewData = false);

        OrderTotalsModel PrepareOrderTotalsModel(IList<ShoppingCartItem> cart, bool isEditable);

        WishlistModel PrepareWishlistModel(WishlistModel model, IList<ShoppingCartItem> cart, bool isEditable = true, int? wishListId = 0, int categoryFilter = -1);

        EstimateShippingResultModel PrepareEstimateShippingResultModel(IList<ShoppingCartItem> cart, int? countryId, int? stateProvinceId, string zipPostalCode, bool cacheOfferedShippingOptions);

        List<CustomerSuspendedCartModel> PrepareCustomerSuspendedCartModel(List<CustomerSuspendedCartModel> model,
            IList<SuspendedCart> cart);
        WishlistModel FilterCategories(WishlistModel model);
        IList<ShoppingCartModel.ShoppingCartItemModel> PrepareSuspendedCartItemsModel(int id);
    }
}
