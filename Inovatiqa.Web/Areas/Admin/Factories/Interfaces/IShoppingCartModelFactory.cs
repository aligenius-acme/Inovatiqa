using Inovatiqa.Database.Models;
using Inovatiqa.Web.Areas.Admin.Models.ShoppingCart;

namespace Inovatiqa.Web.Areas.Admin.Factories.Interfaces
{
    public partial interface IShoppingCartModelFactory
    {
        ShoppingCartSearchModel PrepareShoppingCartSearchModel(ShoppingCartSearchModel searchModel);

        ShoppingCartListModel PrepareShoppingCartListModel(ShoppingCartSearchModel searchModel);

        ShoppingCartItemListModel PrepareShoppingCartItemListModel(ShoppingCartItemSearchModel searchModel, Customer customer);
    }
}