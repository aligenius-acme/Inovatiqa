using Inovatiqa.Database.Models;
using System;
using System.Collections.Generic;

namespace Inovatiqa.Services.Orders.Interfaces
{
    public partial interface IShoppingCartService
    {
        IList<ShoppingCartItem> GetShoppingCart(Customer customer, int shoppingCartTypeId = 0,
            int storeId = 0, int? productId = null, DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int? wishListId = 0, bool getallShoppingCarts = false);

        decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true);

        decimal GetSubTotal(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts,
            out int? maximumDiscountQty);

        decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts = true);

        decimal GetUnitPrice(ShoppingCartItem shoppingCartItem,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts);

        decimal GetUnitPrice(Product product,
            Customer customer,
            int shoppingCartTypeId,
            int quantity,
            string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate, DateTime? rentalEndDate,
            bool includeDiscounts,
            out decimal discountAmount,
            out List<Discount> appliedDiscounts);

        bool ShoppingCartRequiresShipping(IList<ShoppingCartItem> shoppingCart);

        ShoppingCartItem FindShoppingCartItemInTheCart(IList<ShoppingCartItem> shoppingCart,
            int shoppingCartTypeId,
            Product product,
            string attributesXml = "",
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null,
            DateTime? rentalEndDate = null,
            int wishListId = 0);

        IList<string> GetShoppingCartItemWarnings(Customer customer, int shoppingCartTypeId,
            Product product, int storeId,
            string attributesXml, decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, int shoppingCartItemId = 0,
            bool getStandardWarnings = true, bool getAttributesWarnings = true,
            bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
            bool getRentalWarnings = true);

        IList<string> GetRequiredProductWarnings(Customer customer, int shoppingCartTypeId, Product product,
            int storeId, int quantity, bool addRequiredProducts, int shoppingCartItemId);

        IEnumerable<Product> GetProductsRequiringProduct(IList<ShoppingCartItem> cart, Product product);

        IList<string> AddToCart(Customer customer, Product product,
            int shoppingCartTypeId, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, int wishListId = 0, bool reordered = false);

        IList<string> GetStandardWarnings(Customer customer, int shoppingCartTypeId,
            Product product, string attributesXml,
            decimal customerEnteredPrice, int quantity);

        IList<string> GetShoppingCartItemAttributeWarnings(Customer customer,
            int shoppingCartTypeId,
            Product product,
            int quantity = 1,
            string attributesXml = "",
            bool ignoreNonCombinableAttributes = false,
            bool ignoreConditionMet = false);

        void MigrateShoppingCart(Customer fromCustomer, Customer toCustomer, bool includeCouponCodes);

        void DeleteShoppingCartItem(ShoppingCartItem shoppingCartItem, bool resetCheckoutData = true,
            bool ensureOnlyActiveCheckoutAttributes = false);

        void DeleteShoppingCartItem(int shoppingCartItemId, bool resetCheckoutData = true,
            bool ensureOnlyActiveCheckoutAttributes = false);

        IList<string> UpdateShoppingCartItem(Customer customer,
            int shoppingCartItemId, string attributesXml,
            decimal customerEnteredPrice,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool resetCheckoutData = true);

        IList<string> GetShoppingCartWarnings(IList<ShoppingCartItem> shoppingCart,
            string checkoutAttributesXml, bool validateCheckoutAttributes);

        string GetRecurringCycleInfo(IList<ShoppingCartItem> shoppingCart,
            out int cycleLength, out int cyclePeriodId, out int totalCycles);

        IList<WishList> PopulateWishList(Customer customer);
        WishList GetWishList(int? wishListId);


        WishList SaveWishListName(Customer customer, string wishListName, bool isPublic);

        WishList RemoveWishList(Customer customer, int wishListId);

        bool IsWishListShared(Customer customer, int? wishListId);

        SuspendedCart SaveSuspendedShoppingCart(Customer customer, int[] suspendedItemIds, string suspendedCartName, string suspendedCartEmail, string suspendedCartComment);

        IList<SuspendedCart> GetCustomerSuspendedCarts(Customer customer);

        SuspendedCart DeleteSuspendedShoppingCart(Customer customer, int suspendedCartId);

        IList<ShoppingCartItem> GetSuspendedShoppingCart(Customer customer, int suspendedCartId);

        void CopySuspendedItemsToShoppingCartItems(Customer customer, int suspendedCartId);

        void DeleteSuspendedShoppingCartItemById(int id);

        void UpdateSuspendedShoppingCart(SuspendedCart cart);

        SuspendedCart GetSuspendedShoppingCartById(Customer customer, int suspendedCartId);
        int SaveOrderApprovalRequest(IList<ShoppingCartItem> cart, Customer customer);
        IList<ShoppingCartItem> GetAllOrderApprovalItems(int ID);
        SuspendedCart GetOrderWaitingForApproval(int ID);
        bool DeleteOrderFromWaitingList(int ID);
        bool ResetChildShoppingCart(IList<ShoppingCartItem> cart);
        void MarkOrderApproved(int OrderId);
        void UpdatePurchaseStatus(Customer customer);
        void MoveItemsToCurrentUser(List<ShoppingCartItem> items, Customer customer);
    }
}